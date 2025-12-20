using System.Text.Json.Serialization;

namespace Logbook.Graph
{
    /// <summary>
    /// Represents a calendar response from Microsoft Graph
    /// </summary>
    /// <param name="Id">The id of the calendar</param>
    /// <param name="Name">The name of the calendar</param>
    public sealed record Calendar
    (   
        [property: JsonPropertyName("id")]
        string Id,

        [property: JsonPropertyName("name")]
        string Name
    );
}