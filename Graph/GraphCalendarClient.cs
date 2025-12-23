using System.Text.Json;
using Logbook.Models;

namespace Logbook.Graph
{
    /// <summary>
    /// Subclass of graph client that handles all calendar related queries
    /// </summary>
    public class GraphCalendarClient
    {
        readonly GraphClient _graphClient;
        readonly User _user;
        Dictionary<string, string?> cachedCalendarIds = new Dictionary<string, string?>();

        /// <summary>
        /// Creates a new graph calendar client
        /// </summary>
        /// <param name="graphClient">The main graph client</param>
        /// <param name="user">The user the client was made to act on behalf on of</param>
        public GraphCalendarClient(GraphClient graphClient, User user)
        {
            _graphClient = graphClient;
            _user = user;
        }

        /// <summary>
        /// Checks wether a calendar with the specified name exists
        /// </summary>
        /// <param name="calendarName">The name of the calendar</param>
        /// <returns>Wether the calendar exists</returns>
        public async Task<bool> DoesExist(string calendarName)
        {
            string? calendarId = await GetId(calendarName);
            return !string.IsNullOrEmpty(calendarId);
        }

        /// <summary>
        /// Creates a new calendar with the specified name for the user
        /// </summary>
        /// <param name="calendarName">The name of the calendar to create</param>
        /// <returns>Wether the calendar was successfully created or not</returns>
        public async Task<bool> Create(string calendarName)
        {
            if(string.IsNullOrEmpty(calendarName)) return false;
            if (await DoesExist(calendarName)) return false;

            Calendar calendar = new Calendar(null, calendarName);

            string json = JsonSerializer.Serialize(calendar);

            string response = await _graphClient.MakeGraphRequestPost("me/calendars", json);
            calendar = JsonSerializer.Deserialize<Calendar>(response)!;

            if (cachedCalendarIds.ContainsKey(calendarName))
            {
                cachedCalendarIds[calendarName] = calendar.Id;
            }
            else
            {
                cachedCalendarIds.Add(calendarName, calendar.Id);
            }

            return true;
        }

        async Task<string?> GetId(string calendarName)
        {
            if (cachedCalendarIds.TryGetValue(calendarName, out string? id))
            {
                return id;
            }
            else
            {
                string response = await _graphClient.MakeGraphRequestGet($"me/calendars?$filter=name eq '{calendarName}'");

                QueryResponse<Calendar> calendars = JsonSerializer.Deserialize<QueryResponse<Calendar>>(response)!;

                if (calendars.Values.Count == 0)
                {
                    cachedCalendarIds.Add(calendarName, null);
                    return null;
                }

                id = calendars.Values[0].Id;
                cachedCalendarIds.Add(calendarName, id);

                return id;
            }
        }

        /// <summary>
        /// Adds an event to the calendar
        /// </summary>
        /// <param name="calendarName">The name of the calendar to add the event to</param>
        /// <param name="event">The event to add to the calendar</param>
        /// <param name="group">The group the event belongs to</param>
        /// <returns>A bool indicating whether the action was successful</returns>
        public async Task<bool> AddEvent(string calendarName, Models.Event @event, Group group)
        {
            Event graphEvent = new Event(
                null,
                $"{group.EventPrefix}{@event.Title}",
                new EventBody("HTML", "<p>this is some text</p>"),
                new EventTime(@event.StartTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff"), "UTC"),
                new EventTime(@event.EndTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff"), "UTC")
                );

            string json = JsonSerializer.Serialize(graphEvent);

            string? calendarId = await GetId(calendarName);
            if (string.IsNullOrEmpty(calendarId))
            {
                Logger.Log($"Calendar named {calendarName} was not found for user with id {_user.Id}", Logger.LogLevel.Warning);
                return false;
            }

            string response = await _graphClient.MakeGraphRequestPost($"me/calendars/{calendarId}/events", json);

            return true;
        }

        /// <summary>
        /// Updates an existing event in the users calendar
        /// </summary>
        /// <param name="calendarName">The name of the calendar to update the event of</param>
        /// <param name="event">The event to add to the calendar</param>
        /// <param name="group">The group the event belongs to</param>
        /// <param name="id">The id of the event to update</param>
        /// <returns></returns>
        public async Task<bool> UpdateEvent(string calendarName, Models.Event @event, Group group, string id)
        {
            Event graphEvent = new Event(
                null,
                @event.Title,
                new EventBody("HTML", "<p>this is some text</p>"),
                new EventTime(@event.StartTime.ToString(), group.TimeZone),
                new EventTime(@event.EndTime.ToString(), group.TimeZone)
                );

            string json = JsonSerializer.Serialize(graphEvent);

            string? calendarId = await GetId(calendarName);
            if (string.IsNullOrEmpty(calendarId)) return false;

            string response = await _graphClient.MakeGraphRequestPost($"me/calendars/{calendarId}/events/{id}", json);

            return true;
        }

        /// <summary>
        /// Deletes an event in the users calendar
        /// </summary>
        /// <param name="calendarName">The name of the calendar to update the event of</param>
        /// <param name="id">The id of the event to delete</param>
        /// <returns></returns>
        public async Task<bool> DeleteEvent(string calendarName, string id)
        {

            string? calendarId = await GetId(calendarName);
            if (string.IsNullOrEmpty(calendarId)) return false;

            string response = await _graphClient.MakeGraphRequestDelete($"me/calendars/{calendarId}/events/{id}");

            return true;
        }

        /// <summary>
        /// Gets the users calendar events
        /// </summary>
        /// <param name="calendarName">The name of the calendar to get</param>
        /// <returns>The users calendar events</returns>
        public async Task<List<Models.Event>> GetEvents(string calendarName)
        {
            List<Models.Event> events = new List<Models.Event>();

            string? calendarId = await GetId(calendarName);
            if (string.IsNullOrEmpty(calendarId)) return events;

            string eventResponse = await _graphClient.MakeGraphRequestGet($"me/calendars/{calendarId}/events");

            QueryResponse<Event> eventCollection = JsonSerializer.Deserialize<QueryResponse<Event>>(eventResponse)!;
            foreach (Event e in eventCollection.Values)
            {
                events.Add(e.ToLogbookEvent());
            }

            while (!string.IsNullOrEmpty(eventCollection.NextUrl))
            {
                eventResponse = await _graphClient.MakeGraphRequestGet(eventCollection.NextUrl, false);

                eventCollection = JsonSerializer.Deserialize<QueryResponse<Event>>(eventResponse)!;
                foreach (Event e in eventCollection.Values)
                {
                    events.Add(e.ToLogbookEvent());
                }
            }
            return events;
        }
    }
}