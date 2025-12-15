using System.Text.Json;
using Logbook.Data;
using Logbook.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;

namespace Logbook.Controllers
{
    /// <summary>
    /// Controller used to register a user to the application
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class RegisterController : ControllerBase
    {
        IConfiguration _configuration;
        readonly LogbookDBContext _context;
        readonly IDataProtector _dataProtector;
        readonly HttpClient _httpClient;

        const int MAX_STATE_AGE_MINUTES = 5;

        /// <summary>
        /// Creates a new register controller
        /// </summary>
        /// <param name="context">The database context to use</param>
        /// <param name="configuration">The configuration to use</param>
        /// <param name="dataProtectionProvider">The data protection provider to use for this controller</param>
        public RegisterController(LogbookDBContext context, IDataProtectionProvider dataProtectionProvider, IConfiguration configuration)
        {
            _context = context;
            _dataProtector = dataProtectionProvider.CreateProtector("oauth-state");
            _configuration = configuration;
            _httpClient = new HttpClient();
        }


        /// <summary>
        /// Gets the url that the user needs to reach to register themselves
        /// </summary>
        /// <returns>The url the user can reach to register</returns>
        [Authorize]
        public async Task<IActionResult> GetRegisterUrl()
        {
            DTO.TokenCaller? caller = await Util.Auth.GetCallerByHttpContext(HttpContext);
            if (caller == null) return Unauthorized("No valid token was provided");

            User? user = _context.Users.Where(u => u.Id == caller.Id).FirstOrDefault();

            if (user == null)
            {
                //User doesnt exist yet
                user = new User()
                {
                    Id = caller.Id,
                    Username = caller.UserPrincipalName,
                    DisplayName = caller.DisplayName,
                    Enabled = false
                };

                Logger.Log($"Registered new user with Id {user.Id}");

                _context.Users.Add(user);
            }

            _context.SaveChanges();

            DTO.AuthState stateObj = new DTO.AuthState(user.Id, DateTimeOffset.UtcNow);
            string json = JsonSerializer.Serialize(stateObj);
            var state = _dataProtector.Protect(json);

            string uri = $"https://login.microsoftonline.com/{_configuration["AzureAd:TenantId"]}/oauth2/v2.0/authorize"
                + $"?client_id={_configuration["AzureAd:ClientId"]}"
                + "&response_type=code"
                + $"&redirect_uri={_configuration["AzureAd:RedirectUri"]}"
                + "&response_mode=query"
                + $"&scope=offline_access Calendars.ReadWrite"
                + $"&state={state}";

            var data = new
            {
                uri
            };
            return Ok(data);
        }

        /// <summary>
        /// The endpoint the user reaches after having registered with Microsoft
        /// </summary>
        /// <param name="code">The authentication code the user received</param>
        /// <param name="state">The state</param>
        /// <returns>A confirmation that the user is registered</returns>
        [HttpGet("exchange")]
        public async Task<IActionResult> Exchange([FromQuery] string code, string state)
        {
            string stateJson = _dataProtector.Unprotect(state);
            DTO.AuthState authState = JsonSerializer.Deserialize<DTO.AuthState>(stateJson)!;

            //check if state is issued more than five minutes ago
            if (authState.issuedAt.AddMinutes(MAX_STATE_AGE_MINUTES) < DateTimeOffset.UtcNow)
            {
                return Unauthorized("Authorization request has expired");
            }

            User? user = _context.Users.Where(u => u.Id == authState.userId).FirstOrDefault();

            if (user == null)
            {
                return Unauthorized("Invalid state");
            }

            HttpResponseMessage response = await _httpClient.PostAsync($"https://login.microsoftonline.com/{_configuration["AzureAd:TenantId"]}/oauth2/v2.0/token",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["client_id"] = _configuration["AzureAd:ClientId"]!,
                    ["client_secret"] = _configuration["AzureAd:ClientSecret"]!,
                    ["grant_type"] = "authorization_code",
                    ["code"] = code,
                    ["redirect_uri"] = _configuration["AzureAd:RedirectUri"]!,
                    ["scope"] = $"offline_access Calendars.ReadWrite"
                }));

            string contentData = await response.Content.ReadAsStringAsync();
            JsonDocument json = JsonDocument.Parse(contentData);

            string? graphAccessToken = json.RootElement.GetProperty("access_token").GetString();
            string? graphRefreshToken = json.RootElement.GetProperty("refresh_token").GetString();


            user.AccessToken = graphAccessToken!;
            user.RefreshToken = graphRefreshToken!;
            user.Enabled = true;

            _context.SaveChanges();

            string content = """
                <html>
                    <body>
                        <p>You have successfully registered, you can now close this page</p>
                    </body>
                </html>
            """;
            return Content(content, "text/html");
        }
    }
}