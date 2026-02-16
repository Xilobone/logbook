using System.Data;
using ExcelDataReader;
using Logbook.Data;
using Logbook.Models;
using Microsoft.Extensions.Options;
using Microsoft.Kiota.Abstractions.Extensions;

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
                User? sourceUser = context.Users.Where(u => u.Id == group.SourceId).FirstOrDefault();
                if (sourceUser == null)
                {
                    Logger.Log($"Group {group.Name} has sourceId {group.SourceId}, but this user was not found", Logger.LogLevel.Warning);
                    continue;
                }

                Graph.GraphClient graphClient = clientProvider.Create(sourceUser, context);

                byte[] fileBytes = await graphClient.GetOnedriveFile(group.FilePath);

                using MemoryStream stream = new MemoryStream(fileBytes);
                List<Event> events = CreateEventsFromStream(stream, group);

                UpdateEventsInDB(events, context);
            }

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
                        //Start and endtime in utc
                        DateTime startTime = TimeZoneInfo.ConvertTimeToUtc(dateTime.Add(group.StartTime.ToTimeSpan()), TimeZoneInfo.FindSystemTimeZoneById(group.TimeZone));
                        DateTime endTime = TimeZoneInfo.ConvertTimeToUtc(dateTime.Add(group.EndTime.ToTimeSpan()), TimeZoneInfo.FindSystemTimeZoneById(group.TimeZone));

                        RowData rowData = GetRowData(row, headerData);

                        List<EventAttendance> eventAttendances = new List<EventAttendance>();
                        for(int i = 0; i < headerData.NAttendees; i++)
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

        static void UpdateEventsInDB(List<Event> events, LogbookDBContext context)
        {
            if (events.Count == 0) return;

            Logger.Log($"Going to update {events.Count} events");
            //keep track of all events that are in the db but not in the source list, so these can be deleted
            List<Event> allExistingEvents = context.Events.Where(e => e.Group.Id.Equals(events.First().Group.Id)).ToList();

            foreach (Event evnt in events)
            {
                Event? existingEvent = context.Events
                    .Where(e => e.Group.Id.Equals(evnt.Group.Id))
                    .Where(e => e.StartTime.Equals(evnt.StartTime))
                    .Where(e => e.EndTime.Equals(evnt.EndTime))
                    .FirstOrDefault();

                if (existingEvent == null)
                {
                    evnt.Id = new Guid();
                    context.Events.Add(evnt);
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
                context.Events.Remove(oldEvent);
            }

            context.SaveChanges();
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
                TitleColumn = 2,
                OrganizerColumn = 3,
                Attendees = attendees.ToArray(),
                NAttendees = attendees.Count,
                NotesColumn = 4 + attendees.Count
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
                Title = row[headerData.TitleColumn].ToString() ?? "",
                Organizer = row[headerData.OrganizerColumn].ToString() ?? "",
                Notes = row[headerData.NotesColumn].ToString() ?? "",
                AttendanceData = attendanceData
            };
        }

        private class HeaderData
        {
            public int TitleColumn { get; set; } = 0;
            public int OrganizerColumn { get; set; } = 0;
            public int NotesColumn { get; set; } = 0;
            public string[] Attendees { get; set; } = [];
            public int NAttendees { get; set; } = 0;
        }

        private class RowData
        {
            public string Title { get; set; } = "";
            public string Organizer { get; set; } = "";
            public string Notes { get; set; } = "";
            public string[] AttendanceData { get; set; } = [];
        }
    }
}
