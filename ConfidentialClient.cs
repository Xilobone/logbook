using Microsoft.Graph;
using Microsoft.Identity.Client;

public class ConfidentialClient
{
    public static IConfidentialClientApplication Instance
    {
        get
        {
            if (_instance != null) return _instance;
            if (_config == null) throw new InvalidOperationException("Config must be initialized");

            _instance = ConfidentialClientApplicationBuilder.Create(_config["AzureAd:ClientId"])
                .WithClientSecret(_config["AzureAd:ClientSecret"])
                .WithAuthority($"https://login.microsoftonline.com/{_config["AzureAd:TenantId"]}")
                .Build();

            return _instance;
        }
    }

    private static IConfidentialClientApplication? _instance;
    private static IConfiguration? _config;

    public static void Initialize(IConfiguration config)
    {
        _config = config;
    }

        public static GraphServiceClient GetByAccessCode(string? incomingToken)
    {
        if (incomingToken == null) throw new NullReferenceException("No incoming token was provided");

        var credential = new MSALOnBehalfOfCredential(Instance, incomingToken!);

        return new GraphServiceClient(credential);
    }
}
