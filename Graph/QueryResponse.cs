using System.Text.Json.Serialization;

namespace Logbook.Graph
{
    /// <summary>
    /// Represents a generic graph query response
    /// </summary>
    /// <typeparam name="T">The response value type</typeparam>
    /// <param name="Values">The actual values</param>
    /// <param name="NextUrl">The url for the next batch of items, if the query was incomplete</param>
    public sealed record QueryResponse<T>
    (
        [property: JsonPropertyName("value")]
        List<T> Values,

        [property: JsonPropertyName("@odata.nextLink")]
        string? NextUrl
    );
}