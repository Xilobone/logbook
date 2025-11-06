namespace Logbook.Services
{
    public class RefreshCalendarService : BackgroundService
    {
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(3600);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Logger.Log("RefreshService started.");

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

        private Task Refresh()
        {
            // Your refresh logic here
            Logger.Log("Refreshing...");
            return Task.CompletedTask;
        }
    }
}
