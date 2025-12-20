using System.Text.Json.Serialization;

namespace Logbook.Graph
{
    /// <summary>
    /// Represents the result of a drive item request in graph
    /// </summary>
    /// <param name="DownloadUrl">The url where the item can be downloaded</param>
    public sealed record DriveItem
(
    [property: JsonPropertyName("@microsoft.graph.downloadUrl")]
        string DownloadUrl
);
}