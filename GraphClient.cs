using System.IdentityModel.Tokens.Jwt;
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
        static IConfidentialClientApplication ConfidentialApp
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

        /// <summary>
        /// The confidential client application for the authentication app
        /// </summary>
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

        /// <summary>
        /// Initializes the confidential client, must be called before the client can be used
        /// </summary>
        /// <param name="config">The configuration to use</param>
        public static void Initialize(IConfiguration config)
        {
            _config = config;
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

            var credential = new MSALOnBehalfOfCredential(ConfidentialApp, incomingToken!);

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

        /// <summary>
        /// Gets 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<GraphServiceClient> GetGraphClientForUserAsync(LogbookDBContext context, string userId)
        {
            var app = ClientApp;

            var tokenCache = new PersistentTokenCache(context);
            Logger.Log("1");
            tokenCache.SetUserId(userId);
            tokenCache.Enable(app.UserTokenCache);

            var account = await app.GetAccountAsync(userId);

            if (account == null)
                throw new Exception($"No account found in cache for user {userId}");



            var scopes = new[] { "https://graph.microsoft.com/.default" };
            var result = await app.AcquireTokenSilent(scopes, account).ExecuteAsync();

            var authProvider = new BaseBearerTokenAuthenticationProvider(new SimpleAccessTokenProvider(result.AccessToken));

            return new GraphServiceClient(authProvider);
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