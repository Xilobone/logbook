namespace Logbook.Models
{   
    /// <summary>
    /// Represents a user of the program
    /// </summary>
    public class User
    {   
        /// <summary>
        /// The unique identifier of the user
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The display name of the user
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// The entra id corresponding to this user
        /// </summary>
        public Guid EntraId { get; set; } = Guid.Empty;

        /// <summary>
        /// The name of the calendar that is managed
        /// </summary>
        public string CalendarName { get; set; } = string.Empty;

        /// <summary>
        /// Whether the user is enabled and their calendar is updated
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// The groups the user belongs to
        /// </summary>
        public virtual ICollection<Group> Groups { get; set; } = new List<Group>();
    }
}