using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;

namespace Logbook.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        IConfiguration _configuration;
        IConfidentialClientApplication _clientApplication;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;

            _clientApplication = ConfidentialClientApplicationBuilder.Create(_configuration["Client:ClientId"])
                .WithClientSecret(_configuration["Client:ClientSecret"])
                .WithRedirectUri(_configuration["Client:RedirectUri"])
                .WithAuthority(new Uri($"https://login.microsoftonline.com/{_configuration["Client:TenantId"]}/v2.0"))
                .Build();
        }

        [HttpGet("login")]
        public IActionResult Login()
        {

            string uri = $"https://login.microsoftonline.com/{_configuration["Client:TenantId"]}/oauth2/v2.0/authorize"
                + $"?client_id={_configuration["Client:ClientId"]}"
                + "&response_type=code"
                + $"&redirect_uri={_configuration["Client:RedirectUri"]}"
                + "&response_mode=query"
                + $"&scope=api://{_configuration["AzureAd:ClientId"]}/access_as_user offline_access openid profile"
                + "&state=12345";

            return Redirect(uri);
        }

        [HttpGet("redirect")]
        public async Task<IActionResult> Exchange([FromQuery] string code)
        {
            // The scopes must match the API you want to call
            string[] scopes = new[] { $"api://{_configuration["AzureAd:ClientId"]}/access_as_user", "offline_access", "openid", "profile" };

            // Exchange authorization code for token
            var result = await _clientApplication.AcquireTokenByAuthorizationCode(scopes, code).ExecuteAsync();

            var obj = new
            {
                result.AccessToken,
                User = result.Account.Username,
            };

            return Ok(obj);
        }
    }
}