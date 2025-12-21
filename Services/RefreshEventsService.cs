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

        /// <summary>
        /// Creates a new refresh service
        /// </summary>
        /// <param name="serviceProvider">The service provider to use to create scoped contexts</param>
        /// <param name="config">The configuration to use</param>
        public RefreshEventsService(IServiceProvider serviceProvider, IOptionsMonitor<RefreshConfig> config) : base(serviceProvider,config) {}

        /// <summary>
        /// Refreshes the events stored in the database based on the source
        /// </summary>
        /// <returns>A task</returns>
        protected override async Task<Task> Refresh()
        {
            var scope = _serviceProvider.CreateScope();
            LogbookDBContext context = scope.ServiceProvider.GetRequiredService<LogbookDBContext>();
            Graph.GraphClientProvider clientProvider = scope.ServiceProvider.GetRequiredService<Graph.GraphClientProvider>();

            foreach (Group group in context.Groups)
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
                var result = reader.AsDataSet();
                var table = result.Tables[0];

                foreach (DataRow row in table.Rows)
                {

                    if (row[0] is DateTime dateTime)
                    {
                        //Start and endtime in utc
                        DateTime startTime = TimeZoneInfo.ConvertTimeToUtc(dateTime.Add(group.StartTime.ToTimeSpan()), TimeZoneInfo.FindSystemTimeZoneById(group.TimeZone));
                        DateTime endTime = TimeZoneInfo.ConvertTimeToUtc(dateTime.Add(group.EndTime.ToTimeSpan()), TimeZoneInfo.FindSystemTimeZoneById(group.TimeZone));

                        string description = row[1].ToString() ?? "No title";

                        events.Add(new Event()
                        {
                            StartTime = startTime,
                            EndTime = endTime,
                            Title = description,
                            Group = group
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
    }
}
