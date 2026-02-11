namespace Logbook.Models
{
    /// <summary>
    /// Represents a single event template to use for event generation
    /// </summary>
    public class EventTemplate
    {
        /// <summary>
        /// The unique identifier of the event template
        /// </summary>
        public Guid Id {get; set; } 

        /// <summary>
        /// Determines how to show the event in the Outlook calendar, either busy, tentative or free
        /// </summary>
        public EventStatus ShowAs {get; set;} = EventStatus.Busy;
        /// <summary>
        /// The title of the events of this group, supports macros
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// The body of the events of this group, supports macros
        /// </summary>
        public string Body { get; set; } = string.Empty;

        /// <summary>
        /// The default empty template
        /// </summary>
        public static readonly EventTemplate None = new();
    }
}