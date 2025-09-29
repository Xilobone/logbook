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
        /// The id of the group the user belongs to
        /// </summary>
        public Guid GroupId { get; set; } = Guid.Empty;

        /// <summary>
        /// The group the user belongs to
        /// </summary>
        public Group? Group { get; set; }
    }
}