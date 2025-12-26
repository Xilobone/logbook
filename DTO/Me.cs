using System.ComponentModel.DataAnnotations;

namespace Logbook.DTO
{
    /// <summary>
    /// Represents the personal config data that is able to be set
    /// </summary>
    public class Me
    {
        /// <summary>
        /// The display name of the user
        /// </summary>
        [MinLength(1)]
        [MaxLength(50)]
        public string? DisplayName { get; set; }

        /// <summary>
        /// Whether the user has the logbook service enabled
        /// </summary>
        public bool? Enabled { get; set; }

        /// <summary>
        /// The name of the calendar to use for the logbook service
        /// </summary>
        [MinLength(1)]
        [MaxLength(50)]
        public string? CalendarName { get; set; }
    }
}