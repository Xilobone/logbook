// using Logbook.Calendar;
using Logbook.Data;
using Logbook.Models;
using Microsoft.Graph;
using Graph = Microsoft.Graph.Models;

namespace Logbook
{
    /// <summary>
    /// Reads the content of the schedule from the source and updates the stored events accordingly
    /// </summary>
    public class EventUpdater
    {
        readonly GraphServiceClient _graphClient;
        readonly LogbookDBContext _context;

        /// <summary>
        /// Creates a new event updater
        /// </summary>
        /// <param name="graphClient">The graph service client to use to make request</param>
        /// <param name="context">The database context to use</param>
        public EventUpdater(GraphServiceClient graphClient, LogbookDBContext context)
        {
            _graphClient = graphClient;
            _context = context;
        }

        /// <summary>
        /// Updates the events in the database based on the content from the source
        /// </summary>
        public async Task Update()
        {
            foreach (Group group in _context.Groups.ToList())
            {
                List<Event> events = await GetEventsFromSource(group);

                UpdateEventsInDB(events);
            }

        }

        async Task<List<Event>> GetEventsFromSource(Group group)
        {
            Logger.Log("start getting events from source", Logger.LogLevel.Debug);

            byte[] fileBytes = GetFileBytes(group.FilePath);

            Logger.Log("gotten all bytes", Logger.LogLevel.Debug);

            await File.WriteAllBytesAsync("test.xlsx", fileBytes);

            // CalendarManager calendarManager = new CalendarManager(group, _graphClient);

            // using MemoryStream stream = new MemoryStream(fileBytes);
            // List<Event> events = calendarManager.CreateEventsFromStream(stream);

            // return events;

            return new List<Event>();
        }


        byte[] GetFileBytes(string filePath)
        {
   

            return new byte[0];
        }

        void UpdateEventsInDB(List<Event> events)
        {
            if (events.Count == 0) return;

            Logger.Log($"Going to update {events.Count} events");
            //keep track of all events that are in the db but not in the source list, so these can be deleted
            List<Event> allExistingEvents = _context.Events.Where(e => e.Group.Id.Equals(events.First().Group.Id)).ToList();

            foreach (Event evnt in events)
            {
                Event? existingEvent = _context.Events
                    .Where(e => e.Group.Id.Equals(evnt.Group.Id))
                    .Where(e => e.StartTime.Equals(evnt.StartTime))
                    .Where(e => e.EndTime.Equals(evnt.EndTime))
                    .FirstOrDefault();

                if (existingEvent == null)
                {
                    evnt.Id = new Guid();
                    _context.Events.Add(evnt);
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
                _context.Events.Remove(oldEvent);
            }

            _context.SaveChanges();
        }
    }
}