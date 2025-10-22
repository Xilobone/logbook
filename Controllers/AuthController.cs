using Logbook.Data;
using Logbook.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;

namespace Logbook.Controllers
{
    /// <summary>
    /// Api endpoint that handles authentication with the identity provider, is the only enpoint that
    /// is not protected behind authentication (as it would otherwise be inpossible to authenticate)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        IConfiguration _configuration;
        readonly LogbookDBContext _context;

        /// <summary>
        /// Creates a new authentication controller
        /// </summary>
        /// <param name="context">The database context to use</param>
        /// <param name="configuration">The configuration to use</param>
        public AuthController(LogbookDBContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;

            // _clientApplication = ConfidentialClientApplicationBuilder.Create(_configuration["IdentityProvider:ClientId"])
            //     .WithClientSecret(_configuration["IdentityProvider:ClientSecret"])
            //     .WithRedirectUri(_configuration["IdentityProvider:RedirectUri"])
            //     .WithAuthority(new Uri($"https://login.microsoftonline.com/{_configuration["IdentityProvider:TenantId"]}/v2.0"))
            //     .Build();
        }

        /// <summary>
        /// Redirects the user to Microsofts login page
        /// </summary>
        /// <returns></returns>
        [HttpGet("login")]
        public IActionResult Login()
        {
            Logger.Log("Received a login request");

            string uri = $"https://login.microsoftonline.com/{_configuration["IdentityProvider:TenantId"]}/oauth2/v2.0/authorize"
                + $"?client_id={_configuration["IdentityProvider:ClientId"]}"
                + "&response_type=code"
                + $"&redirect_uri={_configuration["IdentityProvider:RedirectUri"]}"
                + "&response_mode=query"
                + $"&scope=offline_access api://{_configuration["AzureAd:ClientId"]}/access_as_user"
                + $"&state=1234";

            return Redirect(uri);
        }

        /// <summary>
        /// Endpoint where the user gets redirected to after having logged in with Microsoft and
        /// accepted the permissions, returns an access token for the api
        /// </summary>
        /// <param name="code">The authorization code</param>
        /// <param name="state">The state, currently unused</param>
        /// <returns>The access token that the user can use to access the api</returns>
        [HttpGet("redirect")]
        public async Task<IActionResult> Exchange([FromQuery] string code, string state)
        {
            string[] scopes = new[] { "offline_access", $"api://{_configuration["AzureAd:ClientId"]}/access_as_user" };

            var tokenCache = new PersistentTokenCache(_context);
            tokenCache.Enable(GraphClient.ClientApp.UserTokenCache);

            // Exchange authorization code for token
            var result = await GraphClient.ClientApp.AcquireTokenByAuthorizationCode(scopes, code).ExecuteAsync();

            tokenCache.SetUserId(result.Account.HomeAccountId.Identifier);

            Guid id = Guid.Parse(result.UniqueId);

            //get stored user info from db
            User? user = _context.Users.Where(user => user.EntraId.Equals(id)).FirstOrDefault();

            //create user if it didnt exist before
            if (user == null)
            {
                user = new User()
                {
                    UserName = result.Account.Username,
                    EntraId = id,
                };

                _context.Users.Add(user);
                _context.SaveChanges();
            }

            var obj = new
            {
                result.AccessToken,
                User = result.Account.Username,
                Id = id
            };

            return Ok(obj);
        }
    }
}