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
        /// A collection of all users in this group
        /// </summary>
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}