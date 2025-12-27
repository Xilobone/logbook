namespace Logbook.DTO
{
    /// <summary>
    /// Represents the state passed through the authentication flow
    /// </summary>
    /// <param name="userId">The id of the user that started the flow</param>
    /// <param name="issuedAt">The time at which the flow was started</param>
    /// <param name="sourceRequest">Whether the request includes a request to be the source</param>
    public record AuthState(Guid userId, DateTimeOffset issuedAt, bool sourceRequest);
}