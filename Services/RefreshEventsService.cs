using System.Data;
using ExcelDataReader;
using Logbook.Data;
using Logbook.Models;
using Microsoft.Extensions.Options;

namespace Logbook.Services
{
    /// <summary>
    /// Service which function is to periodically fetch the event information from the source and
    /// validate if the stored events in the database are up to date
    /// </summary>
    public class RefreshEventsService : RefreshService
    {
        /// <summary>
        /// The name of the config to use
        /// </summary>
        protected override string configName { get => "RefreshEvents"; }

        static Dictionary<string, EventAttendance.AttendanceStatus> AttendanceStatusMap = new Dictionary<string, EventAttendance.AttendanceStatus>()
        {
            {"Ja", EventAttendance.AttendanceStatus.Attending},
            {"Miss", EventAttendance.AttendanceStatus.Tentative},
            {"Nee", EventAttendance.AttendanceStatus.Unavailable},
            {"", EventAttendance.AttendanceStatus.Unknown}
        };

        /// <summary>
        /// Creates a new refresh service
        /// </summary>
        /// <param name="serviceProvider">The service provider to use to create scoped contexts</param>
        /// <param name="config">The configuration to use</param>
        public RefreshEventsService(IServiceProvider serviceProvider, IOptionsMonitor<RefreshConfig> config) : base(serviceProvider, config) { }

        /// <summary>
        /// Refreshes the events stored in the database based on the source
        /// </summary>
        /// <returns>A task</returns>
        protected override async Task<Task> Refresh()
        {
            var scope = _serviceProvider.CreateScope();
            LogbookDBContext context = scope.ServiceProvider.GetRequiredService<LogbookDBContext>();
            Graph.GraphClientProvider clientProvider = scope.ServiceProvider.GetRequiredService<Graph.GraphClientProvider>();

            List<Group> groups = context.Groups.ToList();

            foreach (Group group in groups)
            {
                Logger.Log($"Getting events for group {group.Name}");
                User? sourceUser = context.Users.Where(u => u.Id == group.SourceId).FirstOrDefault();
                if (sourceUser == null)
                {
                    Logger.Log($"Group {group.Name} has sourceId {group.SourceId}, but this user was not found", Logger.LogLevel.Warning);
                    continue;
                }

                if (!sourceUser.OneDriveRegistration.Enabled)
                {
                    Logger.Log($"Group {group.Name} has source user with disabled onedrive registration", Logger.LogLevel.Warning);
                    continue;
                }

                Graph.GraphClient graphClient = clientProvider.Create(sourceUser.OneDriveRegistration, context);

                byte[] fileBytes = await graphClient.GetOnedriveFile(group.FilePath);
                using MemoryStream stream = new MemoryStream(fileBytes);
                List<Event> events = CreateEventsFromStream(stream, group);

                UpdateEventsInDB(events, group, context);
            }

            context.SaveChanges();
            return Task.CompletedTask;
        }

        static List<Event> CreateEventsFromStream(MemoryStream stream, Group group)
        {
            List<Event> events = new List<Event>();

            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                DataSet result = reader.AsDataSet();
                DataTable table = result.Tables[0];

                HeaderData headerData = GetHeaderData(table);

                foreach (DataRow row in table.Rows)
                {

                    if (row[1] is DateTime dateTime)
                    {
                        RowData rowData = GetRowData(row, headerData);

                        if (!rowData.Active) continue;
                        //Start and endtime in utc
                        DateTime startTime = GetUTCTime(rowData.StartTime, rowData.StartDate, group.StartTime, dateTime, group.TimeZone);
                        DateTime endTime = GetUTCTime(rowData.EndTime, rowData.EndDate, group.EndTime, dateTime, group.TimeZone);

                        List<EventAttendance> eventAttendances = new List<EventAttendance>();
                        for (int i = 0; i < headerData.NAttendees; i++)
                        {
                            eventAttendances.Add(new EventAttendance()
                            {
                                Name = headerData.Attendees[i],
                                Status = AttendanceStatusMap[rowData.AttendanceData[i]]
                            });
                        }

                        events.Add(new Event()
                        {
                            StartTime = startTime,
                            EndTime = endTime,
                            Title = rowData.Title,
                            Group = group,
                            Notes = rowData.Notes,
                            Organizer = rowData.Organizer,
                            EventAttendances = eventAttendances
                        });
                    }
                    else continue;
                }
            }

            return events;
        }

        static void UpdateEventsInDB(List<Event> events, Group group, LogbookDBContext context)
        {
            if (events.Count == 0) return;

            Logger.Log($"Going to update {events.Count} events");
            //keep track of all events that are in the db but not in the source list, so these can be deleted
            List<Event> allExistingEvents = group.Events.ToList();

            foreach (Event evnt in events)
            {
                Event? existingEvent = group.Events
                    .Where(e => e.StartTime.Equals(evnt.StartTime))
                    .FirstOrDefault(e => e.EndTime.Equals(evnt.EndTime));

                if (existingEvent == null)
                {
                    evnt.Id = new Guid();
                    group.Events.Add(evnt);
                    Logger.Log($"added {evnt.Title}");
                }
                else if (!existingEvent.Equals(evnt))
                {
                    //event from source differs from stored event in db, update it
                    Logger.Log($"{evnt} differs from {existingEvent}");

                    //update fields
                    existingEvent.Title = evnt.Title;
                    existingEvent.Notes = evnt.Notes;
                    existingEvent.Organizer = evnt.Organizer;
                    existingEvent.EventAttendances = evnt.EventAttendances;


                    allExistingEvents.Remove(existingEvent);
                }
                else
                {
                    allExistingEvents.Remove(existingEvent);

                }
            }

            foreach (Event oldEvent in allExistingEvents)
            {
                group.Events.Remove(oldEvent);
                Logger.Log($"removed {oldEvent.Title}");
            }

            // context.SaveChanges();

        }

        static DateTime GetUTCTime(TimeOnly? time, DateTime? date, TimeOnly defaultTime, DateTime defaultDate, string timeZone)
        {
            DateTime d = date ?? defaultDate;
            TimeOnly t = time ?? defaultTime;
            return TimeZoneInfo.ConvertTimeToUtc(d.Add(t.ToTimeSpan()), TimeZoneInfo.FindSystemTimeZoneById(timeZone));
        }

        static HeaderData GetHeaderData(DataTable table)
        {
            List<string> attendees = new List<string>();

            int col = 4;
            bool cont = true;
            while (cont)
            {
                string? attendee = table.Rows[2][col].ToString();

                if (string.IsNullOrEmpty(attendee))
                {
                    cont = false;
                    continue;
                }

                attendees.Add(attendee);
                col++;
            }

            return new HeaderData()
            {
                ActiveColumn = 0,
                TitleColumn = 2,
                OrganizerColumn = 3,
                Attendees = attendees.ToArray(),
                NAttendees = attendees.Count,
                NotesColumn = attendees.Count + 4,
                StartTimeColumn = attendees.Count + 5,
                StartDateColumn = attendees.Count + 7,
                EndTimeColumn = attendees.Count + 6,
                EndDateColumn = attendees.Count + 8,
            };
        }

        static RowData GetRowData(DataRow row, HeaderData headerData)
        {
            string[] attendanceData = new string[headerData.NAttendees];
            for (int attendanceColumn = 0; attendanceColumn < headerData.NAttendees; attendanceColumn++)
            {
                attendanceData[attendanceColumn] = row[4 + attendanceColumn].ToString() ?? "";
            }

            return new RowData()
            {
                Active = (row[headerData.ActiveColumn].ToString() ?? "False").Equals("True"),
                Title = row[headerData.TitleColumn].ToString() ?? "",
                Organizer = row[headerData.OrganizerColumn].ToString() ?? "",
                Notes = row[headerData.NotesColumn].ToString() ?? "",
                AttendanceData = attendanceData,
                StartTime = ParseTime(row[headerData.StartTimeColumn]),
                StartDate = ParseDate(row[headerData.StartDateColumn].ToString()),
                EndTime = ParseTime(row[headerData.EndTimeColumn]),
                EndDate = ParseDate(row[headerData.EndDateColumn].ToString()),
            };
        }

        static TimeOnly? ParseTime(object? cell)
        {
            if (cell == null || cell == DBNull.Value)
                return null;

            // ExcelDataReader often gives DateTime for time cells
            if (cell is DateTime dt)
                return TimeOnly.FromDateTime(dt);

            // Sometimes Excel gives a double for time-only cells
            if (cell is double d)
            {
                // Excel stores time as fraction of a day
                var dt2 = DateTime.FromOADate(d);
                return TimeOnly.FromDateTime(dt2);
            }

            // Fallback: parse as text
            if (TimeOnly.TryParse(cell.ToString(), out var time))
                return time;

            return null;
        }


        static DateTime? ParseDate(string? text)
        {
            if (text == null) return null;

            if (DateTime.TryParse(text, out DateTime date)) return date;
            return null;
        }

        private class HeaderData
        {
            public int ActiveColumn { get; set; } = 0;
            public int TitleColumn { get; set; } = 0;
            public int OrganizerColumn { get; set; } = 0;
            public int NotesColumn { get; set; } = 0;
            public int StartTimeColumn { get; set; } = 0;
            public int StartDateColumn { get; set; } = 0;
            public int EndTimeColumn { get; set; } = 0;
            public int EndDateColumn { get; set; } = 0;
            public string[] Attendees { get; set; } = [];
            public int NAttendees { get; set; } = 0;
        }

        private class RowData
        {
            public bool Active {get; set; } = false;
            public string Title { get; set; } = "";
            public string Organizer { get; set; } = "";
            public string Notes { get; set; } = "";
            public string[] AttendanceData { get; set; } = [];

            public TimeOnly? StartTime { get; set; } = null;
            public DateTime? StartDate { get; set; } = null;
            public TimeOnly? EndTime { get; set; } = null;
            public DateTime? EndDate { get; set; } = null;

        }
    }
}
