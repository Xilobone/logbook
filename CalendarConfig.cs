namespace Logbook
{   
    /// <summary>
    /// Contains the configuration of the calendar
    /// </summary>
    public class CalendarConfig
    {
        /// <summary>
        /// The standard start time of the events
        /// </summary>
        public TimeSpan StartTime { get; set; } = TimeSpan.MinValue;

        /// <summary>
        /// The standard end time of the events
        /// </summary>
        public TimeSpan EndTime { get; set; } = TimeSpan.MinValue;

        /// <summary>
        /// The time zone of the events in the calendar
        /// </summary>
        public string TimeZone { get; init; } = string.Empty;
    }
}