using Azure.Core;
using Microsoft.Identity.Client;
/// <summary>
/// Responsible for handling the requesting of access tokens to Microsoft Graph on behalf of the user (OBO)
/// </summary>
public class MSALOnBehalfOfCredential : TokenCredential
{
    private readonly IConfidentialClientApplication _app;
    private readonly string _incomingToken;

    /// <summary>
    /// Creates a new OBO requester with the application and incoming token
    /// </summary>
    /// <param name="app">The representation of client application that corresponds with this program</param>
    /// <param name="incomingToken">The incoming token the user has obtained from authenticating with Microsoft</param>
    public MSALOnBehalfOfCredential(IConfidentialClientApplication app, string incomingToken)
    {
        _app = app;
        _incomingToken = incomingToken;
    }

    /// <summary>
    /// Requests an access token to Microsoft Graph on behalf of the user
    /// </summary>
    /// <param name="requestContext">The context of the request</param>
    /// <param name="cancellationToken">The cancellation token to use for the request</param>
    /// <returns>The obtained access token</returns>
    public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        return GetTokenAsync(requestContext, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Requests an access token to Microsoft Graph on behalf of the user asynchronously
    /// </summary>
    /// <param name="requestContext">The context of the request</param>
    /// <param name="cancellationToken">The cancellation token to use for the request</param>
    /// <returns>The obtained access token</returns>
    public override async ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        string[] graphScopes = new[] { "https://graph.microsoft.com/.default" };

        var result = await _app
            .AcquireTokenOnBehalfOf(graphScopes, new UserAssertion(_incomingToken))
            .ExecuteAsync(cancellationToken);

        // var result = await _app.AcquireTokenByAuthorizationCode(graphScopes, _incomingToken).ExecuteAsync();

        return new AccessToken(result.AccessToken, result.ExpiresOn);
    }
}
