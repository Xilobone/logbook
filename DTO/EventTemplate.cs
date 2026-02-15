
using System.ComponentModel.DataAnnotations;

namespace Logbook.DTO
{
    /// <summary>
    /// Represents an event template as a DTO object
    /// </summary>
    public class EventTemplate
    {
        /// <summary>
        /// Determines how to show the event in the Outlook calendar, either busy, tentative or free
        /// </summary>
        [RegularExpression("^(?i)(busy|free|tentative)$")]
        public string ShowAs {get; set;} = "busy";
        /// <summary>
        /// The title of the events of this group, supports macros
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// The body of the events of this group, supports macros
        /// </summary>
        public string Body { get; set; } = string.Empty;

        /// <summary>
        /// The default empty template
        /// </summary>
        public static readonly EventTemplate None = new();
    }


}