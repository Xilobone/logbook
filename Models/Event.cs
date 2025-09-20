using System.ComponentModel.DataAnnotations;

namespace Logbook.Models
{
    /// <summary>
    /// Represents an event in a calendar
    /// </summary>
    public class Event
    {
        /// <summary>
        /// The id of the event
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// The start date and time of the event
        /// </summary>
        public DateTime StartTime { get; set; } = DateTime.UnixEpoch;

        /// <summary>
        /// The end date and time of the event
        /// </summary>
        public DateTime EndTime { get; set; } = DateTime.UnixEpoch;

        /// <summary>
        /// The Title of the event
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Creates a string representation of the event
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"({StartTime} - {EndTime}) {Title}";
        }
        
    }
}