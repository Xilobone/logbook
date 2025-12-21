namespace Logbook.Models
{
    /// <summary>
    /// Resembles a group of which users can be part, groups can influence the content visible for the users
    /// </summary>
    public class Group
    {
        /// <summary>
        /// The unique identifier of this group
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The display name of this group
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The path to the schedule file on the source onedrive
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// The user id of the source
        /// </summary>
        public Guid SourceId {get; set; } = Guid.Empty;
        
        /// <summary>
        /// The default start time of the events in this group
        /// </summary>
        public TimeOnly StartTime { get; set; } = TimeOnly.MinValue;

        /// <summary>
        /// The default end time of events in this group
        /// </summary>
        public TimeOnly EndTime { get; set; } = TimeOnly.MaxValue;

        /// <summary>
        /// The timezone the calendar events should be in
        /// </summary>
        public string TimeZone { get; set; } = string.Empty;

        /// <summary>
        /// The text to write before the event title
        /// </summary>
        public string EventPrefix {get; set;} = string.Empty;
        /// <summary>
        /// A collection of all users in this group
        /// </summary>
        public virtual ICollection<User> Users { get; set; } = new List<User>();

        /// <summary>
        /// A collection of events in this group
        /// </summary>
        public virtual ICollection<Event> Events { get; set; } = new List<Event>();

        /// <summary>
        /// Empty Group to indicate no group is present
        /// </summary>
        public static readonly Group None = new();
    }
}