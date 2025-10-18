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
        IConfidentialClientApplication _clientApplication;
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

            _clientApplication = ConfidentialClientApplicationBuilder.Create(_configuration["IdentityProvider:ClientId"])
                .WithClientSecret(_configuration["IdentityProvider:ClientSecret"])
                .WithRedirectUri(_configuration["IdentityProvider:RedirectUri"])
                .WithAuthority(new Uri($"https://login.microsoftonline.com/{_configuration["IdentityProvider:TenantId"]}/v2.0"))
                .Build();
        }

        [HttpGet("login")]
        public IActionResult Login()
        {
            Logger.Log("Received a login request");

            AuthState authState = new AuthState()
            {
                GroupId = _context.Groups.Where(g => g.Name.Equals("Welpen")).Select(g => g.Id).FirstOrDefault()
            };

            string uri = $"https://login.microsoftonline.com/{_configuration["IdentityProvider:TenantId"]}/oauth2/v2.0/authorize"
                + $"?client_id={_configuration["IdentityProvider:ClientId"]}"
                + "&response_type=code"
                + $"&redirect_uri={_configuration["IdentityProvider:RedirectUri"]}"
                + "&response_mode=query"
                + $"&scope=api://{_configuration["AzureAd:ClientId"]}/.default"
                + $"&state={AuthState.Encode(authState)}";

            return Redirect(uri);
        }

        [HttpGet("redirect")]
        public async Task<IActionResult> Exchange([FromQuery] string code, string state)
        {
            AuthState authState = AuthState.Decode(state);

            //Create group if it didnt exist before
            bool groupExists = _context.Groups.Any(g => g.Id == authState.GroupId);

            if (!groupExists)
            {
                _context.Groups.Add(new Group()
                {
                    Name = "Welpen",
                });

                _context.SaveChanges();
            }

            string[] scopes = new[] { $"api://{_configuration["AzureAd:ClientId"]}/.default" };

            // Exchange authorization code for token
            var result = await _clientApplication.AcquireTokenByAuthorizationCode(scopes, code).ExecuteAsync();

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