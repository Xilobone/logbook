namespace Logbook.DTO
{   
    /// <summary>
    /// Represents a personal event template set
    /// </summary>
    public class PersonalEventTemplateSet
    {
        /// <summary>
        /// Determines whether the event template set is enabled
        /// </summary>
        public bool Enabled {get; set;} = false;

        /// <summary>
        /// The actual event template set
        /// </summary>
        public EventTemplateSet EventTemplateSet {get; set;} = EventTemplateSet.None;
    }
}