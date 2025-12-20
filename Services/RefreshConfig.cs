namespace Logbook.Services
{
    /// <summary>
    /// Configuration for the refresh service
    /// </summary>
    public class RefreshConfig
    {
        /// <summary>
        /// Wether the service is enabled
        /// </summary>
        public bool enabled {get; set; } = false;
        /// <summary>
        /// The refresh interval, in seconds
        /// </summary>
        public int interval { get; set; } = 0;

        /// <summary>
        /// The initual delay before starting the service, in seconds
        /// </summary>
        public int delay { get; set; } = 0;
    }
}