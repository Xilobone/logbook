namespace Logbook.Models
{   
    /// <summary>
    /// Represents a personal event template set
    /// </summary>
    public class PersonalEventTemplateSet
    {   
        /// <summary>
        /// The unique identifier of the personal event template set
        /// </summary>
        public Guid Id { get; set; } = Guid.Empty;

        /// <summary>
        /// Whether the set is enabled and overriding the group template set
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// The user this set belongs to
        /// </summary>
        public virtual User User { get; set; } = User.None;

        /// <summary>
        /// The group that this set applies to
        /// </summary>
        public virtual Group Group { get; set; } = Group.None;

        /// <summary>
        /// The event template set
        /// </summary>
        public virtual EventTemplateSet EventTemplateSet { get; set; } = EventTemplateSet.None;

        /// <summary>
        /// Used to represent a default personal event template set
        /// </summary>
        public static PersonalEventTemplateSet None { get; } = new();
    }
}