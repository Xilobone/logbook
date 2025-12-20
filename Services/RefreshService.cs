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
    public abstract class RefreshService : BackgroundService
    {
        /// <summary>
        /// The name of the configuration to be loaded from the appsettings
        /// </summary>
        protected abstract string configName { get; }

        /// <summary>
        /// The service provider to create scoped services
        /// </summary>
        protected readonly IServiceProvider _serviceProvider;

        readonly bool _enabled;
        readonly TimeSpan _interval;
        readonly TimeSpan _delay;

        /// <summary>
        /// Creates a new refresh service
        /// </summary>
        /// <param name="serviceProvider">The service provider to use to create scoped contexts</param>
        /// <param name="config">The configuration to use</param>
        public RefreshService(IServiceProvider serviceProvider, IOptionsMonitor<RefreshConfig> config)
        {
            _serviceProvider = serviceProvider;
            _interval = TimeSpan.FromSeconds(config.Get(configName).interval);
            _delay = TimeSpan.FromSeconds(config.Get(configName).delay);
            _enabled = config.Get(configName).enabled;
        }

        /// <summary>
        /// Starts the refreshing progress, calls Refresh() whenever the interval passed
        /// </summary>
        /// <param name="stoppingToken">The token that stops execution of the refresh</param>
        /// <returns>A task that will only conclude when the stopping token is triggered</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_enabled)
            {
                Logger.Log($"RefreshService({configName}) is not enabled");

                return;
            }
            await Task.Delay(_delay, stoppingToken);

            Logger.Log($"RefreshService({configName}) started after an initial delay of {_delay.TotalSeconds} seconds, with interval of {_interval.TotalSeconds} seconds.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Refresh();

                    Logger.Log($"Refresh({configName}) completed at: {DateTimeOffset.Now}");
                }
                catch (Exception ex)
                {
                    Logger.Log(ex, Logger.LogLevel.Error);
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }

        /// <summary>
        /// Method that gets called whenever the interval has passed
        /// </summary>
        /// <returns>A task completed</returns>
        protected abstract Task<Task> Refresh();

    }
}
