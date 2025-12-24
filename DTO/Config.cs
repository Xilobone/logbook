using System.ComponentModel.DataAnnotations;

namespace Logbook.DTO
{
    /// <summary>
    /// Represents the data that can be passed to the update personal config endpoint
    /// </summary>
    public class PersonalConfig
    {
        /// <summary>
        /// The display name of the user
        /// </summary>
        [MinLength(1)]
        [MaxLength(50)]
        public string? displayName { get; set; }

        /// <summary>
        /// Whether the user has the logbook service enabled
        /// </summary>
        public bool? enabled { get; set; }

        /// <summary>
        /// The name of the calendar to use for the logbook service
        /// </summary>
        [MinLength(1)]
        [MaxLength(50)]
        public string? calendarName { get; set; }
    }
}