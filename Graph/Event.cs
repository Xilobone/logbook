using System.Text.Json.Serialization;

namespace Logbook.Graph
{   
    /// <summary>
    /// Represents an event from Microsoft Graph
    /// </summary>
    /// <param name="Id">The id of the event</param>
    /// <param name="Subject">The subject of the event</param>
    /// <param name="Body">The body of the event</param>
    /// <param name="Start">The start time of the event</param>
    /// <param name="End">The end time of the event</param>
    public sealed record Event
    (
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("subject")] string Subject,
        [property: JsonPropertyName("body")] EventBody Body,
        [property: JsonPropertyName("start")] EventTime Start,
        [property: JsonPropertyName("end")] EventTime End

    );

    /// <summary>
    /// Represents the body of an event from Microsoft Graph
    /// </summary>
    /// <param name="Content">The content of the body</param>
    public sealed record EventBody
    (
        [property: JsonPropertyName("content")] string Content
    );

    /// <summary>
    /// Represents a time from an event from Microsoft Graph
    /// </summary>
    /// <param name="DateTime">The date time of the event</param>
    /// <param name="timeZone">The timezone the date time is given in</param>
    public sealed record EventTime
(
    [property: JsonPropertyName("dateTime")] string DateTime,
    [property: JsonPropertyName("timeZone")] string timeZone
);
}