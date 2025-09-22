using System.Globalization;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Graph = Microsoft.Graph.Models;

namespace Logbook.Calendar
{
    public class Calendar
    {
        readonly GraphServiceClient _graphClient;
        readonly string _id;

        const string SPELTAK = "Welpen";
        public static string TIMEZONE = "UTC";

        /// <summary>
        /// Creates a new calendar object, all events in the calendar are fetched from Graph
        /// </summary>
        /// <param name="graphClient">The graph client to use to make requests to Microsoft Graph</param>
        /// <param name="calendarId">The id of the corresponing calendar in outlook</param>
        public Calendar(GraphServiceClient graphClient, string calendarId)
        {
            _graphClient = graphClient;
            _id = calendarId;
        }

        /// <summary>
        /// Gets a list of all events that are currently in the calendar
        /// </summary>
        /// <returns>A list of all events in the calendar</returns>
        public async Task<List<Models.Event>> GetAllEvents()
        {
            List<Models.Event> events = new List<Models.Event>();

            EventCollectionResponse? currentResponse = await _graphClient
                .Me.Calendars[_id]
                .Events
                .GetAsync();

            while (currentResponse != null && currentResponse.Value != null)
            {
                // Add current page of events
                foreach (Graph.Event e in currentResponse.Value)
                {
                    events.Add(ToLogbookEvent(e));
                }

                // Move to the next page if one exists
                if (!string.IsNullOrEmpty(currentResponse.OdataNextLink))
                {
                    Logger.Log($"Fetching next page: {currentResponse.OdataNextLink}");

                    currentResponse = await _graphClient
                        .Me.Calendars[_id]
                        .Events
                        .WithUrl(currentResponse.OdataNextLink)
                        .GetAsync();
                }
                else
                {
                    // No more pages
                    break;
                }
            }

            return events;
        }

        /// <summary>
        /// Adds an event to the calendar
        /// </summary>
        /// <param name="event">The event to add</param>
        /// <returns>The id of the added event</returns>
        public async Task<string> AddEvent(Models.Event @event)
        {
            Graph.Event e = ToGraphEvent(@event);
            Graph.Event? createdEvent = await _graphClient.Me.Calendars[_id].Events.PostAsync(e);

            if (createdEvent == null) return string.Empty;

            Logger.Log($"Created event with id (${createdEvent.Id})");
            return createdEvent.Id!;
        }

        public async Task<string> UpdateEvent(Models.Event @event, string eventId)
        {
            Graph.Event e = ToGraphEvent(@event);
            Graph.Event? updatedEvent = await _graphClient.Me.Calendars[_id].Events[eventId].PatchAsync(e);

            if (updatedEvent == null) return string.Empty;

            Logger.Log($"Updated event with id (${updatedEvent.Id})");
            return updatedEvent.Id!;
        }

        public async void DeleteEvent(string eventId)
        {
            await _graphClient.Me.Calendars[_id].Events[eventId].DeleteAsync();

            return;
        }

        /// <summary>
        /// Tries to find the id of the corresponing event in the list of events
        /// </summary>
        /// <param name="evnt">The event to search for</param>
        /// <param name="events">The list of all events</param>
        /// <returns>The id of the event if it was found, or null otherwise</returns>
        public static string FindEventId(Models.Event evnt, List<Models.Event> events)
        {
            Logger.Log($"trying to find an event with name:{evnt.Title}", Logger.LogLevel.Warning);
            Logger.Log($"trying to find an event with starttime:{evnt.StartTime}", Logger.LogLevel.Warning);
            Logger.Log($"trying to find an event with endtime:{evnt.EndTime}", Logger.LogLevel.Warning);
            string? id = events
                .Where(
                    e => e.StartTime.Equals(evnt.StartTime) &&
                    e.EndTime.Equals(evnt.EndTime) &&
                    e.Title.Equals(evnt.Title))
                .Select(e => e.Id)
                .FirstOrDefault();

            return id ?? string.Empty;
        }

        private Models.Event ToLogbookEvent(Graph.Event @event)
        {
            string title = @event.Subject != null ? @event.Subject.Replace($"Opkomst {SPELTAK}: ", "") : "";
            
            return new Models.Event
            {
                Id = @event.Id ?? string.Empty,
                StartTime = DateTime.Parse(@event.Start!.DateTime!),
                EndTime = DateTime.Parse(@event.End!.DateTime!),
                Title = title
            };
        }

        private Graph.Event ToGraphEvent(Models.Event @event)
        {
            return new Event()
            {
                Subject = $"Opkomst {SPELTAK}: {@event.Title}",
                Body = new ItemBody
                {
                    ContentType = BodyType.Html,
                    Content = $"<h1>{@event.Title}</h1><p>This is some text too</p>"
                },
                Start = new DateTimeTimeZone()
                {
                    // "o" indicates the right ISO standard
                    DateTime = @event.StartTime.ToString("o"),
                    TimeZone = TIMEZONE
                },
                End = new DateTimeTimeZone()
                {
                    DateTime = @event.EndTime.ToString("o"),
                    TimeZone = TIMEZONE
                }
            };
        }
    }
}