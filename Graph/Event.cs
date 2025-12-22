using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][property: JsonPropertyName("id")] string? Id,
        [property: JsonPropertyName("subject")] string Subject,
        [property: JsonPropertyName("body")] EventBody Body,
        [property: JsonPropertyName("start")] EventTime Start,
        [property: JsonPropertyName("end")] EventTime End

    )
    {
        /// <summary>
        /// Creates a Logbook model event based on the graph event info
        /// </summary>
        /// <returns>A logbook event</returns>
        public Models.Event ToLogbookEvent()
        {
            TimeZoneInfo startTimezone = TimeZoneInfo.FindSystemTimeZoneById(Start.timeZone);
            DateTime startTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.Parse(Start.DateTime), startTimezone);

            TimeZoneInfo endTimezone = TimeZoneInfo.FindSystemTimeZoneById(End.timeZone);
            DateTime endTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.Parse(End.DateTime), endTimezone);

            Models.Event @event = new Models.Event()
            {
                Title = Subject,
                StartTime = startTime,
                EndTime = endTime,
                CalendarEventId = Id,
            };

            return @event;
        }
    };

    /// <summary>
    /// Represents the body of an event from Microsoft Graph
    /// </summary>
    /// <param name="ContentType">The content type of the body</param>
    /// <param name="Content">The content of the body</param>
    public sealed record EventBody
    (
        [property: JsonPropertyName("contentType")] string ContentType,
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