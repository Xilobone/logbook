using System.Text.Json.Serialization;

namespace Logbook.Models
{   
    /// <summary>
    /// The possible statusses of an event in a calendar
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum EventStatus
    {
        /// <summary>
        /// Corresponds to the 'busy' state in Outlook
        /// </summary>
        Busy,

        /// <summary>
        /// Corresponds to the 'Tentative' state in Outlook
        /// </summary>
        Tentative,

        /// <summary>
        /// Corresponds to the 'free' state in Outlook
        /// </summary>
        Free,

        /// <summary>
        /// Indicates that the event should not be in the agenda
        /// </summary>
        NotIncluded
    }
}