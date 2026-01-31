using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using Logbook.Data;
using Logbook.Models;
using Microsoft.Extensions.Options;

namespace Logbook.Services
{
    /// <summary>
    /// Service which function is to periodically validate the users calendar events
    /// against the database and update the calendar accordingly
    /// </summary>
    public class RefreshCalendarsService : RefreshService
    {
        /// <summary>
        /// The name of the config to use
        /// </summary>
        protected override string configName { get => "RefreshCalendars"; }

        /// <summary>
        /// Creates a new refresh calendars service
        /// </summary>
        /// <param name="serviceProvider">The service provider to use to create scoped contexts</param>
        /// <param name="config">The configuration to use</param>
        public RefreshCalendarsService(IServiceProvider serviceProvider, IOptionsMonitor<RefreshConfig> config) : base(serviceProvider, config) { }

        /// <summary>
        /// Refreshes the events in the users calendars based on the events in the database
        /// </summary>
        /// <returns>A task completed</returns>
        protected override async Task<Task> Refresh()
        {
            var scope = _serviceProvider.CreateScope();
            LogbookDBContext context = scope.ServiceProvider.GetRequiredService<LogbookDBContext>();
            Graph.GraphClientProvider clientProvider = scope.ServiceProvider.GetRequiredService<Graph.GraphClientProvider>();

            List<User> users = context.Users.ToList();
            foreach (User user in users)
            {
                Logger.Log($"Going to update events of user {user.Id}");
                if (!user.Enabled) continue;

                Graph.GraphClient graphClient = clientProvider.Create(user, context);

                if (!await graphClient.Calendars.DoesExist(user.CalendarName))
                {
                    Logger.Log($"No calendar named {user.CalendarName} exists for user {user.Id}, creating a new calendar");
                    bool successful = await graphClient.Calendars.Create(user.CalendarName);
                    if (!successful) continue;
                }

                //get all events from the users calendar
                List<Graph.Event> calendarEvents = await graphClient.Calendars.GetEvents(user.CalendarName);

                //keep track of all events that are no longer found in the db so we can delete them at last
                List<Graph.Event> unmatchedEvents = [.. calendarEvents];
                Logger.Log($"Found {calendarEvents.Count} events in the users calendar");
                //for each group they are part of get the events of that group from the database and check if that event does exist
                //in the users calendar, create it if it doesnt, or update it if necessary

                List<Models.Group> groups = user.Groups.ToList();
                foreach (Models.Group group in user.Groups)
                {
                    List<Event> groupEvents = group.Events.ToList();

                    foreach (Event @event in groupEvents)
                    {
                        Graph.Event? existingEvent = calendarEvents
                            .Where(e => e.Subject.Equals(Macros.Fill(group.EventTitle, @event)))
                            .Where(e => DoesBodyMatch(e, @event, group))
                            .Where(e => DoesTimeMatch(e.Start,@event.StartTime))
                            .FirstOrDefault(e => DoesTimeMatch(e.End,@event.EndTime));

                        if (existingEvent == null)
                        {
                            Logger.Log($"Adding event {@event.Title} to user {user.Id} calendar");
                            await graphClient.Calendars.AddEvent(user.CalendarName, @event, group);
                        }
                        else
                        {


                            unmatchedEvents.Remove(existingEvent);
                            //update existing event
                            //for now the only fields in the event are the key fields, so events will always be identical or not be matched
                        }
                    }
                }

                foreach (Graph.Event @event in unmatchedEvents)
                {
                    Logger.Log($"Deleting event {@event.Subject} from user {user.Id} calendar");
                    await graphClient.Calendars.DeleteEvent(user.CalendarName, @event.Id!);
                }
            }
            return Task.CompletedTask;
        }

        bool DoesBodyMatch(Graph.Event graphEvent, Models.Event @event, Models.Group group)
        {
            string graphBody = NormalizeHtml(graphEvent.Body.Content);
            string eventBody = Macros.Fill(group.EventBody, @event);

            return graphBody.Equals(eventBody);
        }

        bool DoesTimeMatch(Graph.EventTime graphTime, DateTime eventTime)
        {
            DateTime graphDateTime = DateTime.Parse(graphTime.DateTime, CultureInfo.InvariantCulture);
            return (graphDateTime.Equals(eventTime));
        }

        static string NormalizeHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;

            // Extract body content if it exists
            var bodyMatch = Regex.Match(
                html,
                @"<body[^>]*>(.*?)</body>",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            string content = bodyMatch.Success
                ? bodyMatch.Groups[1].Value
                : html;

            // Remove head/meta if still present
            content = Regex.Replace(content, @"<head[^>]*>.*?</head>", "",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // Normalize whitespace between tags and text
            content = Regex.Replace(content, @">\s+<", "><"); // between tags
            content = Regex.Replace(content, @"\s+", " ");    // inside text

            return content.Trim();
        }
    }
}
