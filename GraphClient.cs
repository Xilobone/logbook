using Microsoft.Graph;
public class GraphClient
{
    public static GraphServiceClient GetByAccessCode(string? incomingToken)
    {
        if (incomingToken == null) throw new NullReferenceException("No incoming token was provided");

        var credential = new MsalOnBehalfOfCredential(ConfidentialClient.Instance, incomingToken!);

        return new GraphServiceClient(credential);
    }
}