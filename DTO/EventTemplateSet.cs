namespace Logbook.DTO
{
    /// <summary>
    /// Represents an eventemplate set in DTO
    /// </summary>
    public class EventTemplateSet
    {
        /// <summary>
        /// Whether to use different templates depending on the attendance, if 
        /// false only Attending will be used
        /// </summary>
        public bool DifferentiateOnAttendance { get; set; } = false;

        /// <summary>
        /// The event template for when the user is attending the event
        /// </summary>
        public virtual EventTemplate? Attending { get; set; } = EventTemplate.None;

        /// <summary>
        /// The event template for when the user may be attending the event
        /// </summary>
        public virtual EventTemplate? Tentative { get; set; } = EventTemplate.None;

        /// <summary>
        /// The event template when the user is unavailable to attend the event
        /// </summary>
        public virtual EventTemplate? Unavailable { get; set; } = EventTemplate.None;

        /// <summary>
        /// The default empty template set
        /// </summary>
        public static readonly EventTemplateSet None = new();
    }
}