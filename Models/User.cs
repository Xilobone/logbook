using System.Text.RegularExpressions;

namespace Logbook.Models
{
    /// <summary>
    /// Represents a user of the program
    /// </summary>
    public class User
    {
        /// <summary>
        /// The different matching types for the users alias
        /// </summary>
        public enum AliasMatching
        {
            /// <summary>
            /// Matches only fields that are exactly equal to the alias
            /// </summary>
            Strict,
            /// <summary>
            /// Matches all fields that contain the alias
            /// </summary>
            Loose,

            /// <summary>
            /// Matches all fields that match the regular expression
            /// </summary>
            Regex
        }

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
        /// The users alias in schedule files
        /// </summary>
        public string Alias { get; set; } = string.Empty;

        /// <summary>
        /// The type of matching that will be used for the alias
        /// </summary>
        public AliasMatching AliasMatchingType { get; set; } = AliasMatching.Strict;
        /// <summary>
        /// The name of the calendar that is managed
        /// </summary>
        public string CalendarName { get; set; } = string.Empty;

        // /// <summary>
        // /// Whether the user is enabled and their calendar is updated
        // /// </summary>
        // public bool Enabled { get; set; } = false;

        /// <summary>
        /// The users registration for accessing the calendar
        /// </summary>
        public virtual Registration CalendarRegistration {get; set;} = Registration.None;

        /// <summary>
        /// The users registration for accessing onedrive
        /// </summary>
        public virtual Registration OneDriveRegistration {get; set;} = Registration.None;
        // /// <summary>
        // /// Whether the user can be the source for the schedule files
        // /// </summary>
        // public bool CanBeSource { get; set; } = false;

        // /// <summary>
        // /// The account name of the linked onedrive account
        // /// </summary>
        // public string LinkedOneDriveAccount {get; set;} = string.Empty;

        /// <summary>
        /// The groups the user belongs to
        /// </summary>
        public virtual ICollection<Group> Groups { get; set; } = new List<Group>();

        // /// <summary>
        // /// Indicates whether the user has an outlook calendar linked
        // /// </summary>
        // public bool HasCalendarLinked {get; set; } = false;

        // /// <summary>
        // /// The account name of the linked calendar account
        // /// </summary>
        // public string LinkedCalendarAccount {get; set;} = string.Empty;
        // /// <summary>
        // /// The accessToken assosiated with this user
        // /// </summary>
        // public string AccessToken { get; set; } = string.Empty;

        // /// <summary>
        // /// The refreshToken assosiated with this user
        // /// </summary>
        // public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// Collection of all personal event templates of the user
        /// </summary>
        public virtual ICollection<PersonalEventTemplateSet> PersonalEventTemplates {get; set;} = new List<PersonalEventTemplateSet>();

        /// <summary>
        /// Represents a default empty user
        /// </summary>
        public static User None {get;} = new();
        
        /// <summary>
        /// Checks if a given string matches the users alias, respects the set matching type
        /// </summary>
        /// <param name="text">The text to check agains the users alias</param>
        /// <returns>True if the text matches, false otherwise</returns>
        public bool IsAnAliasMatch(string text)
        {
            switch(AliasMatchingType)
            {
                case AliasMatching.Loose:
                    return Alias.Contains(text);
                case AliasMatching.Strict:
                    return Alias.Equals(text);
                case AliasMatching.Regex:
                    return Regex.Match(text,Alias).Success;
                default:
                    return false;
            }
        }
    }
}