using System.Data;
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

            foreach (User user in context.Users)
            {   
                Logger.Log($"Going to update events of user {user.Id}");
                if (!user.Enabled) continue;

                Graph.GraphClient graphClient = clientProvider.Create(user, context);

                if (!await graphClient.Calendars.DoesExist(user.CalendarName))
                {
                    Logger.Log($"No calendar named {user.CalendarName} exists for user {user.Id}, creating a new calendar");
                    await graphClient.Calendars.Create(user.CalendarName);
                }

                //get all events from the users calendar
                List<Event> calendarEvents = await graphClient.Calendars.GetEvents(user.CalendarName);

                //keep track of all events that are no longer found in the db so we can delete them at last
                List<Event> unmatchedEvents = [.. calendarEvents];

                //for each group they are part of get the events of that group from the database and check if that event does exist
                //in the users calendar, create it if it doesnt, or update it if necessary
                foreach (Group group in user.Groups)
                {   
                    List<Event> groupEvents = group.Events.ToList();

                    foreach (Event @event in groupEvents)
                    {   
                        Event? existingEvent = calendarEvents
                            .Where(e => e.Title.Equals($"{group.EventPrefix}{@event.Title}"))
                            .Where(e => e.StartTime.Equals(@event.StartTime))
                            .FirstOrDefault(e => e.EndTime.Equals(@event.EndTime));

                        if (existingEvent == null)
                        {   
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

                foreach (Event @event in unmatchedEvents)
                {
                    Logger.Log($"Deleting event {@event.Title} from user {user.Id} calendar");
                    await graphClient.Calendars.DeleteEvent(user.CalendarName, @event.CalendarEventId!);
                }
            }
            return Task.CompletedTask;
        }
    }
}
