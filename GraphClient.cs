using System.IdentityModel.Tokens.Jwt;
using Logbook.Data;
using Microsoft.Graph;
using Microsoft.Identity.Client;

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

        private static IConfidentialClientApplication? _instance;
        private static IConfiguration? _config;

        /// <summary>
        /// Initializes the confidential client, must be called before the client can be used
        /// </summary>
        /// <param name="config"></param>
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

            var credential = new MSALOnBehalfOfCredential(ConfidentialClient, incomingToken!);

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
    }
}