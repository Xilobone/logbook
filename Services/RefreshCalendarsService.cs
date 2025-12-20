using System.Data;
using ExcelDataReader;
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
        public RefreshCalendarsService(IServiceProvider serviceProvider, IOptionsMonitor<RefreshConfig> config) : base(serviceProvider,config) {}

        /// <summary>
        /// Refreshes the events in the users calendars based on the events in the database
        /// </summary>
        /// <returns>A task completed</returns>
        protected override async Task<Task> Refresh()
        {
            var scope = _serviceProvider.CreateScope();
            LogbookDBContext context = scope.ServiceProvider.GetRequiredService<LogbookDBContext>();
            GraphClientProvider clientProvider = scope.ServiceProvider.GetRequiredService<GraphClientProvider>();

            return Task.CompletedTask;
        }
    }
}
