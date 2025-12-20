using System.Text.Json.Serialization;

namespace Logbook.Graph
{   
    /// <summary>
    /// Represents a token response from the Graph token endpoint
    /// </summary>
    /// <param name="AccessToken">The access token</param>
    /// <param name="RefreshToken">The refresh token</param>
    /// <param name="ExpiresIn">The time at which the token expires</param>
    /// <param name="TokenType">The type of token, generally 'Bearer'</param>
    public sealed record TokenResponse
    (
        [property: JsonPropertyName("access_token")]
        string AccessToken,

        [property: JsonPropertyName("refresh_token")]
        string RefreshToken,

        [property: JsonPropertyName("expires_in")]
        int ExpiresIn,

        [property: JsonPropertyName("token_type")]
        string TokenType
    );
}