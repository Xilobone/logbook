// using System.IdentityModel.Tokens.Jwt;
// using Logbook.Data;
// using Logbook.Models;
// using Microsoft.Graph;
// using Microsoft.Identity.Client;
// using Microsoft.Kiota.Abstractions.Authentication;

// namespace Logbook
// {
//     /// <summary>
//     /// Used to obtain a graph service client that can query data from Microsoft Graph
//     /// </summary>
//     public class GraphClient
//     {
//         IConfidentialClientApplication ConfidentialApp
//         {
//             get
//             {
//                 if (_instance != null) return _instance;
//                 if (_config == null) throw new InvalidOperationException("Config must be initialized");

//                 _instance = ConfidentialClientApplicationBuilder.Create(_config["AzureAd:ClientId"])
//                     .WithClientSecret(_config["AzureAd:ClientSecret"])
//                     .WithAuthority($"https://login.microsoftonline.com/{_config["AzureAd:TenantId"]}")
//                     .Build();

//                 return _instance;
//             }
//         }

//         /// <summary>
//         /// The confidential client application for the authentication app
//         /// </summary>
//         public IConfidentialClientApplication ClientApp
//         {
//             get
//             {
//                 if (_clientApp != null) return _clientApp;
//                 if (_config == null) throw new InvalidOperationException("Config must be initialized");

//                 _clientApp = ConfidentialClientApplicationBuilder.Create(_config["IdentityProvider:ClientId"])
//                         .WithClientSecret(_config["IdentityProvider:ClientSecret"])
//                         .WithRedirectUri(_config["IdentityProvider:RedirectUri"])
//                         .WithAuthority(new Uri($"https://login.microsoftonline.com/{_config["IdentityProvider:TenantId"]}/v2.0"))
//                         .Build();


//                 return _clientApp;
//             }
//         }


//         IConfidentialClientApplication? _clientApp;
//         private IConfidentialClientApplication? _instance;
//         private IConfiguration _config;

//         LogbookDBContext _context;

//         /// <summary>
//         /// Creates a new graph client
//         /// </summary>
//         /// <param name="context">The database context to use</param>
//         /// <param name="config">The configuration to use</param>
//         public GraphClient(LogbookDBContext context, IConfiguration config)
//         {
//             _context = context;
//             _config = config;
//         }

//         /// <summary>
//         /// Creates a Microsoft Graph service client based on the incoming token from the authenticated user
//         /// </summary>
//         /// <param name="incomingToken">The access token of the user</param>
//         /// <returns>A graph service client, used to query graph data</returns>
//         /// <exception cref="NullReferenceException">If no incoming token was provided</exception>
//         public GraphServiceClient GetByAccessCode(string? incomingToken, Guid userId = default)
//         {
//             Logger.Log("called get by access code");
//             if (incomingToken == null) throw new NullReferenceException("No incoming token was provided");

//             var credential = new MSALOnBehalfOfCredential(CreateConfidentialApp(userId), incomingToken!);

//             return new GraphServiceClient(credential);
//         }

//         /// <summary>
//         /// Gets the entra id of the user that provided the token
//         /// </summary>
//         /// <param name="incomingToken">The incoming token provided</param>
//         /// <returns>The entra id of the user</returns>
//         public Guid GetUserEntraId(string incomingToken)
//         {
//             var handler = new JwtSecurityTokenHandler();
//             var jwt = handler.ReadJwtToken(incomingToken);
//             string? oid = jwt.Claims.FirstOrDefault(c => c.Type == "oid")?.Value;
//             Guid entraId = Guid.Parse(oid!);

//             return entraId;
//         }

//         /// <summary>
//         /// Creates a graph service client based on the users stored token cache
//         /// </summary>
//         /// <param name="context">The database context to use</param>
//         /// <param name="user">The user to get the token cache of</param>
//         /// <returns>A graph service client</returns>
//         /// <exception cref="Exception">Throws an exception if no cache was found, or if the cache is expired</exception>
//         public async Task<GraphServiceClient> GetGraphClientForUserAsync(LogbookDBContext context, User user)
//         {
//             var app = CreateConfidentialApp(user.Id);


//             // var account = await app.GetAccountAsync(user.Id.ToString());
//             // if (account == null)
//             //     throw new Exception($"No account found in cache for user {user.Id}");

//             var scopes = new[] { "https://graph.microsoft.com/.default" };
//             // var result = await app.AcquireTokenSilent(scopes, user).ExecuteAsync();

//             // var authProvider = new BaseBearerTokenAuthenticationProvider(new SimpleAccessTokenProvider(result.AccessToken));

//             // return new GraphServiceClient(authProvider);
//             return null;
//         }

//         IConfidentialClientApplication CreateConfidentialApp(Guid userId = default)
//         {
//             var app = ConfidentialClientApplicationBuilder.Create(_config["AzureAd:ClientId"])
//                 .WithClientSecret(_config["AzureAd:ClientSecret"])
//                 .WithAuthority($"https://login.microsoftonline.com/{_config["AzureAd:TenantId"]}")
//                 .Build();

//             PersistentTokenCache tokenCache = new PersistentTokenCache(_context);
//             tokenCache.Enable(app.UserTokenCache, userId);
//             return app;
//         }
//     }

// }

// internal class MsalAccount : IAccount
// {
//         public MsalAccount()
//         {
//         }

//         public MsalAccount(string objectId, string tenantId)
//         {
//             HomeAccountId = new AccountId($"{objectId}.{tenantId}", objectId, tenantId);
//         }

//         public string Username { get; set; }

//         public string Environment { get; set; }

//         public AccountId HomeAccountId { get; set; }
// }
// internal class SimpleAccessTokenProvider : IAccessTokenProvider
// {
//     private readonly string _token;
//     public SimpleAccessTokenProvider(string token) => _token = token;

//     /// <summary>
//     /// Gets an authorization token
//     /// </summary>
//     /// <param name="uri">The uri</param>
//     /// <param name="additionalAuthenticationContext">Additional context</param>
//     /// <param name="cancellationToken">The cancellation token</param>
//     /// <returns>The autorization token</returns>
//     public Task<string> GetAuthorizationTokenAsync(
//         Uri uri,
//         Dictionary<string, object>? additionalAuthenticationContext = null,
//         CancellationToken cancellationToken = default)
//     {
//         return Task.FromResult(_token);
//     }

//     public AllowedHostsValidator AllowedHostsValidator { get; } = new();
// }
