using System.IdentityModel.Tokens.Jwt;
using Azure.Identity;
using Logbook.Data;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Microsoft.Kiota.Abstractions.Authentication;

namespace Logbook
{
    /// <summary>
    /// Used to obtain a graph service client that can query data from Microsoft Graph
    /// </summary>
    public class GraphClient
    {
        static IConfidentialClientApplication ConfidentialClient
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

        public static IConfidentialClientApplication ClientApp
        {
            get
            {
                if (_clientApp != null) return _clientApp;
                if (_config == null) throw new InvalidOperationException("Config must be initialized");

                _clientApp = ConfidentialClientApplicationBuilder.Create(_config["IdentityProvider:ClientId"])
                        .WithClientSecret(_config["IdentityProvider:ClientSecret"])
                        .WithRedirectUri(_config["IdentityProvider:RedirectUri"])
                        .WithAuthority(new Uri($"https://login.microsoftonline.com/{_config["IdentityProvider:TenantId"]}/v2.0"))
                        .Build();

                return _clientApp;
            }
        }

        static IConfidentialClientApplication? _clientApp;
        private static IConfidentialClientApplication? _instance;
        private static IConfiguration? _config;

        public static IServiceProvider? _serviceProvider;

        /// <summary>
        /// Initializes the confidential client, must be called before the client can be used
        /// </summary>
        /// <param name="config"></param>
        public static void Initialize(IConfiguration config, IServiceProvider serviceProvider)
        {
            _config = config;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Creates a Microsoft Graph service client based on the incoming token from the authenticated user
        /// </summary>
        /// <param name="incomingToken">The access token of the user</param>
        /// <returns>A graph service client, used to query graph data</returns>
        /// <exception cref="NullReferenceException">If no incoming token was provided</exception>
        public static GraphServiceClient GetByAccessCode(string? incomingToken)
        {
            if (incomingToken == null) throw new NullReferenceException("No incoming token was provided");

            var credential = new MSALOnBehalfOfCredential(ConfidentialClient, incomingToken!);

            // var tokenCache = new PersistentTokenCache(_serviceProvider, GetUserEntraId(incomingToken).ToString());
            // tokenCache.Enable(ConfidentialClient.UserTokenCache);
            // tokenCache.Enable(ClientApp.UserTokenCache);

            return new GraphServiceClient(credential);
        }

        /// <summary>
        /// Gets the entra id of the user that provided the token
        /// </summary>
        /// <param name="incomingToken">The incoming token provided</param>
        /// <returns>The entra id of the user</returns>
        public static Guid GetUserEntraId(string incomingToken)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(incomingToken);
            string? oid = jwt.Claims.FirstOrDefault(c => c.Type == "oid")?.Value;
            Guid entraId = Guid.Parse(oid!);

            return entraId;
        }

        public static async Task<GraphServiceClient> GetGraphClientForUserAsync(LogbookDBContext context, string userId)
        {
            // 1️⃣ Get the shared ConfidentialClient
            var app = GraphClient.ClientApp;

            // 2️⃣ Attach the user's token cache from DB
            var tokenCache = new PersistentTokenCache(context);
            Logger.Log("1");
            tokenCache.SetUserId(userId);
            tokenCache.Enable(app.UserTokenCache);

            var account = await app.GetAccountAsync(userId);

            // 3️⃣ Acquire token silently using the user's account info
            var accounts = await app.GetAccountsAsync();
            // var account = accounts.FirstOrDefault(a => a.HomeAccountId.Identifier == userId);

            if (account == null)
                throw new Exception($"No account found in cache for user {userId}");

            // var result = await app.AcquireTokenSilent(
            //     new[] { "https://graph.microsoft.com/.default" },
            //     account
            // ).ExecuteAsync();

            Logger.Log("going great");
            // 4️⃣ Create Graph client with this token
            // var credential = new OnBehalfOfCredential(_config["AzureAd:TentantId"], _config["AzureAd:ClientId"], _config["AzureAd:ClientSecret"], result.AccessToken);

            // var credential = new MSALOnBehalfOfCredential(app, userId);
            // Logger.Log("created credential");
            // var graphClient = new GraphServiceClient(credential);
            var scopes = new[] { "https://graph.microsoft.com/.default" };
            var result = await app.AcquireTokenSilent(scopes, account).ExecuteAsync();

            // Create an authentication provider manually
            var authProvider = new BaseBearerTokenAuthenticationProvider(new SimpleAccessTokenProvider(result.AccessToken));

            // Create the GraphServiceClient
            var graphClient = new GraphServiceClient(authProvider);
            return graphClient;
        }

    }

    internal class SimpleAccessTokenProvider : IAccessTokenProvider
    {
        private readonly string _token;
        public SimpleAccessTokenProvider(string token) => _token = token;

        public Task<string> GetAuthorizationTokenAsync(
            Uri uri,
            Dictionary<string, object> additionalAuthenticationContext = default,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_token);
        }

        public AllowedHostsValidator AllowedHostsValidator { get; } = new();
    }
}