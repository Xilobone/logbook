using System.Text.Json.Serialization;
using Logbook.Models;

namespace Logbook.Graph
{
    /// <summary>
    /// Represents a user obtained from graph
    /// </summary>
    /// <param name="Id">The id of the user </param>
    /// <param name="UserPrincipalName">The unique name of the user</param>
    /// <param name="DisplayName">The display name of the user</param>
    public sealed record User
    (
        [property: JsonPropertyName("id")] string? Id,
        [property: JsonPropertyName("userPrincipalName")] string UserPrincipalName,
        [property: JsonPropertyName("displayName")] string DisplayName
    );

}