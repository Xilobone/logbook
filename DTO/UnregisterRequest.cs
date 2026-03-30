using System.Text.Json.Serialization;

namespace Logbook.DTO
{
    /// <summary>
    /// The parameters that need to be passed in order to remove a registration
    /// </summary>
    public class UnregisterRequest
    {
        /// <summary>
        /// Whether the unregister request is for the source registration or not
        /// </summary>
        [JsonPropertyName("source")]
        public bool Source {get; set;} = false;
    }
}