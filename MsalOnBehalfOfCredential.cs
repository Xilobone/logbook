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
        string[] graphScopes = new[] { "https://graph.microsoft.com/.default" };
        var result = await _app
            .AcquireTokenOnBehalfOf(graphScopes, new UserAssertion(_incomingToken))
            .ExecuteAsync(cancellationToken);

        Console.WriteLine(result.AccessToken);
        
        return new AccessToken(result.AccessToken, result.ExpiresOn);
    }
}
