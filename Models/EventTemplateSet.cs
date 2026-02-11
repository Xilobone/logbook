namespace Logbook.Models
{
    /// <summary>
    /// Represents an event template set to use for event generation
    /// </summary>
    public class EventTemplateSet
    {
        /// <summary>
        /// The unique identifier of the event template set
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The group the event template set belongs to
        /// </summary>
        public virtual Group Group { get; set; } = Group.None;

        /// <summary>
        /// The user the event template set belongs to, can be null if the template is a group default
        /// </summary>
        public virtual User? User { get; set; }

        /// <summary>
        /// Whether to use different templates depending on the attendance, if 
        /// false only Attending will be used
        /// </summary>
        public bool DifferentiateOnAttendance { get; set; } = false;

        /// <summary>
        /// The event template for when the user is attending the event
        /// </summary>
        public virtual EventTemplate Attending { get; set; } = EventTemplate.None;

        /// <summary>
        /// The event template for when the user may be attending the event
        /// </summary>
        public virtual EventTemplate Tentative { get; set; } = EventTemplate.None;

        /// <summary>
        /// The event template when the user is unavailable to attend the event
        /// </summary>
        public virtual EventTemplate Unavailable { get; set; } = EventTemplate.None;

        /// <summary>
        /// The default empty template set
        /// </summary>
        public static readonly EventTemplateSet None = new();

    }
}