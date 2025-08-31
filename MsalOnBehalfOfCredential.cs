using Azure.Core;
using Microsoft.Identity.Client;

public class MsalOnBehalfOfCredential : TokenCredential
{
    private readonly IConfidentialClientApplication _app;
    private readonly string _incomingToken;

    public MsalOnBehalfOfCredential(IConfidentialClientApplication app, string incomingToken)
    {
        _app = app;
        _incomingToken = incomingToken;
    }

    public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        => GetTokenAsync(requestContext, cancellationToken).GetAwaiter().GetResult();

    public override async ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        var result = await _app
            .AcquireTokenOnBehalfOf(requestContext.Scopes, new UserAssertion(_incomingToken))
            .ExecuteAsync(cancellationToken);

        return new AccessToken(result.AccessToken, result.ExpiresOn);
    }
}
