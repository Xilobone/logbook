using System.Data;
using ExcelDataReader;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Graph = Microsoft.Graph.Models;

namespace Logbook.Calendar
{   
    /// <summary>
    /// This class serves as a way to create and update calendars
    /// </summary>
    public class CalendarManager
    {
        readonly GraphServiceClient _graphClient;

        /// <summary>
        /// Creates a new calendar manager, responsible for creating and updating calendars
        /// </summary>
        /// <param name="graphClient">The graph service client used for interacting with Microsoft Graph</param>
        public CalendarManager(GraphServiceClient graphClient)
        {
            _graphClient = graphClient;
        }

        /// <summary>
        /// Creates a new calendar with the given name, if no such calendar exists yet
        /// </summary>
        /// <param name="name">The name of the calendar to create</param>
        /// <returns>The id of the calendar</returns>
        public async Task<string?> GetOrCreateCalendar(string name)
        {

            //return existing calendar id if it was found
            string? existingId = await GetCalendar(name);

            if (existingId != null)
            {
                Logger.Log($"A calendar named {name} already exists, no new calendar is created");
                return existingId;
            }

            //Create new calendar
            Graph.Calendar calendar = new Graph.Calendar();
            calendar.Name = name;

            Graph.Calendar? addedCalendar = await _graphClient.Me.Calendars.PostAsync(calendar);

            if (addedCalendar == null) return null;

            return addedCalendar.Id;
        }

        /// <summary>
        /// Gets the id of the calendar with the given name, if it exists
        /// </summary>
        /// <param name="name">The name of the calendar</param>
        /// <returns>The id of the calendar, if it exists, null otherwise</returns>
        public async Task<string?> GetCalendar(string name)
        {
            //get all calendars
            CalendarCollectionResponse? calendarResponse = await _graphClient.Me.Calendars.GetAsync();
            if (calendarResponse == null || calendarResponse.Value == null) return null;

            //select existing calendar with matching name
            Graph.Calendar? existingCalendar = calendarResponse.Value
                .Where(calendar => calendar.Name == name)
                .FirstOrDefault();

            return existingCalendar != null ? existingCalendar.Id : null;
        }

        /// <summary>
        /// Updates all events in the specified calendar to match the given events, will delete
        /// all not matching events
        /// </summary>
        /// <param name="calendarId">The id of the calendar to update the events in</param>
        /// <param name="events">The list of events to update in the calendar</param>
        public async Task UpdateCalendar(string calendarId, List<Models.Event> events)
        {
            Logger.Log(events, "All events obtained from the document");

            Calendar calendar = new Calendar(_graphClient, calendarId);

            // EventCollectionResponse? eventsResponse = await _graphClient.Me.Calendars[calendarId].Events.GetAsync();
            List<Models.Event> existingEvents = await calendar.GetAllEvents();
            Logger.Log(existingEvents, "All events currently in the agenda");

            // if (eventsResponse == null || eventsResponse.Value == null) return;

            // Logger.Log(eventsResponse.Value, "All events currently in the agenda",
            //     e => $"({e.Start!.DateTime} - {e.End!.DateTime}) {e.Subject}");

            List<Models.Event> deletedEvents = [.. existingEvents];

            foreach (Models.Event evnt in events)
            {
                //check if event is already present, if so update if required, if not create event
                string existingEventId = calendar.FindEventId(evnt, existingEvents);
                string id;
                if (string.IsNullOrEmpty(existingEventId))
                {
                    id = await calendar.AddEvent(evnt);
                }
                else
                {
                    id = await calendar.UpdateEvent(evnt, existingEventId);
                }

                //mark this event as not to be deleted
                deletedEvents.RemoveAll(e => e.Id!.Equals(id));
            }

            //delete all events that have not just been checked, updated or created
            foreach (Models.Event e in deletedEvents)
            {
                // calendar.DeleteEvent(e.Id);
            }
        }

        /// <summary>
        /// Creates a list of events based on the contents of a stream, stream must be a valid xlsx file
        /// </summary>
        /// <param name="stream">The memorystream to read from</param>
        /// <returns>A list of calendar events</returns>
        public static List<Models.Event> CreateEventsFromStream(MemoryStream stream)
        {
            List<Models.Event> events = new List<Models.Event>();

            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                var result = reader.AsDataSet();
                var table = result.Tables[0];

                foreach (DataRow row in table.Rows)
                {

                    if (row[0] is DateTime dateTime)
                    {
                        DateTime startTime = dateTime.AddHours(10);
                        DateTime endTime = dateTime.AddHours(12);

                        string description = row[1].ToString() ?? "No title";

                        events.Add(new Models.Event()
                        {
                            StartTime = startTime,
                            EndTime = endTime,
                            Title = description
                        });
                    }
                    else continue;
                }
            }

            return events;
        }
    }
}