using Logbook.Data;
using Logbook.Models;
using Microsoft.Extensions.Options;
using Microsoft.Graph;

namespace Logbook.Services
{   
    /// <summary>
    /// Service which function is to periodically fetch the event information from the source and
    /// validate if the stored events in the database are up to date
    /// </summary>
    public class RefreshCalendarService : BackgroundService
    {
        readonly IServiceProvider _serviceProvider;
        readonly TimeSpan _interval;

        /// <summary>
        /// Creates a new refresh service
        /// </summary>
        /// <param name="serviceProvider">The service provider to use to create scoped contexts</param>
        /// <param name="config">The configuration to use</param>
        public RefreshCalendarService(IServiceProvider serviceProvider, IOptions<RefreshConfig> config)
        {
            _serviceProvider = serviceProvider;
            _interval = TimeSpan.FromSeconds(config.Value.interval);
        }

        /// <summary>
        /// Starts the refreshing progress
        /// </summary>
        /// <param name="stoppingToken">The token that stops execution of the refresh</param>
        /// <returns>A task that will only conclude when the stopping token is triggered</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Logger.Log($"RefreshService started, with interval of {_interval.TotalSeconds} seconds.");

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(_interval, stoppingToken);
                try
                {
                    await Refresh();

                    Logger.Log($"Refresh completed at: {DateTimeOffset.Now}");
                }
                catch (Exception ex)
                {
                    Logger.Log(ex, Logger.LogLevel.Error);
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }

        private async Task<Task> Refresh()
        {   
            var scope = _serviceProvider.CreateScope();
            LogbookDBContext context = scope.ServiceProvider.GetRequiredService<LogbookDBContext>();
            GraphClient graphClient = scope.ServiceProvider.GetRequiredService<GraphClient>();
            foreach(User user in context.Users)
            {   
                GraphServiceClient graphSClient = await graphClient.GetGraphClientForUserAsync(context, user);

                EventUpdater eventUpdater = new EventUpdater(graphSClient, context);
                await eventUpdater.Update();
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Configuration for the refresh service
        /// </summary>
        public class RefreshConfig
        {   
            /// <summary>
            /// The refresh interval, in seconds
            /// </summary>
            public int interval {get; set;} = 0;
        }
    }
}
