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
}
