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

        public Calendar(GraphServiceClient graphClient, string calendarId)
        {
            _graphClient = graphClient;
            _id = calendarId;
        }

        public async Task<List<Models.Event>> GetAllEvents()
        {
            List<Models.Event> events = new List<Models.Event>();

            EventCollectionResponse? eventsResponse = await _graphClient.Me.Calendars[_id].Events.GetAsync();

            if (eventsResponse == null || eventsResponse.Value == null) return events;

            foreach (Graph.Event e in eventsResponse.Value)
            {
                events.Add(new Models.Event()
                {
                    Id = e.Id ?? string.Empty,
                    StartTime = DateTime.Parse(e.Start!.DateTime!),
                    EndTime = DateTime.Parse(e.End!.DateTime!),
                    Title = e.Subject ?? "No subject"
                });
            }

            string? nextLink = eventsResponse.OdataNextLink;
            while (!string.IsNullOrEmpty(nextLink))
            {
                Logger.Log(nextLink);
                EventCollectionResponse? response = await _graphClient.Me.Calendars[_id].Events.WithUrl(eventsResponse.OdataNextLink).GetAsync();

                if (response == null || response.Value == null) return events;
                nextLink = response.OdataNextLink;

                foreach (Graph.Event e in response.Value)
                {
                    events.Add(new Models.Event()
                    {
                        Id = e.Id ?? string.Empty,
                        StartTime = DateTime.Parse(e.Start!.DateTime!),
                        EndTime = DateTime.Parse(e.End!.DateTime!),
                        Title = e.Subject ?? "No subject"
                    });
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

        public async Task<string> FindEventId(Models.Event evnt, string calendarId)
        {
            List<Models.Event> events = await GetAllEvents();

            string? id = events
                .Where(
                    e => e.StartTime.Equals(evnt.StartTime) &&
                    e.EndTime.Equals(evnt.EndTime) &&
                    e.Title.Equals(evnt.Title))
                .Select(e => e.Id)
                .FirstOrDefault();

            return id ?? string.Empty;
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
                    TimeZone = "UTC"
                },
                End = new DateTimeTimeZone()
                {
                    DateTime = @event.EndTime.ToString("o"),
                    TimeZone = "UTC"
                }
            };
        }
    }
}