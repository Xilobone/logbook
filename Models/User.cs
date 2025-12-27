using System.ComponentModel.DataAnnotations.Schema;

namespace Logbook.Models
{
    /// <summary>
    /// Represents a user of the program
    /// </summary>
    public class User
    {
        /// <summary>
        /// The unique identifier of the user, corresponds with the Entra ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The username of the user, will be their email
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// The display name of the user, will be their first and last name
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// The name of the calendar that is managed
        /// </summary>
        public string CalendarName { get; set; } = string.Empty;

        /// <summary>
        /// Whether the user is enabled and their calendar is updated
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// Whether the user can be the source for the schedule files
        /// </summary>
        public bool CanBeSource {get; set; } = false;

        /// <summary>
        /// The groups the user belongs to
        /// </summary>
        public virtual ICollection<Group> Groups { get; set; } = new List<Group>();

        /// <summary>
        /// The accessToken assosiated with this user
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// The refreshToken assosiated with this user
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;
    }
}