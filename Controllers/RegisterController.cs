using System.Text.Json;
using System.Text.Json.Serialization;
using Logbook.Data;
using Logbook.DTO;
using Logbook.Graph;
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
        readonly GraphClientProvider _graphClientProvider;
        readonly HttpClient _httpClient;

        const int MAX_STATE_AGE_MINUTES = 5;

        /// <summary>
        /// Creates a new register controller
        /// </summary>
        /// <param name="context">The database context to use</param>
        /// <param name="configuration">The configuration to use</param>
        /// <param name="dataProtectionProvider">The data protection provider to use for this controller</param>
        /// <param name="graphClientProvider">The graph client provider to use for this controller</param>
        public RegisterController(LogbookDBContext context, IDataProtectionProvider dataProtectionProvider, IConfiguration configuration, GraphClientProvider graphClientProvider)
        {
            _context = context;
            _dataProtector = dataProtectionProvider.CreateProtector("oauth-state");
            _configuration = configuration;
            _graphClientProvider = graphClientProvider;
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// Gets the url that the user needs to reach to link a calendar or onedrive account
        /// </summary>
        /// <returns>The url the user can reach to link a calendar or onedrive account</returns>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetLinkAccountUrl([FromQuery] bool source = false)
        {
            (bool isValidRequest, Models.User user, IActionResult error) = await Util.Auth.ValidateRequest(this, _context);
            if (!isValidRequest) return error;

            DTO.TokenCaller? caller = await Util.Auth.GetCallerByHttpContext(HttpContext);
            if (caller == null) return Unauthorized("No valid token was provided");

            DTO.AuthState stateObj = new DTO.AuthState(user.Id, DateTimeOffset.UtcNow, source);
            string json = JsonSerializer.Serialize(stateObj);
            var state = _dataProtector.Protect(json);

            string uri = $"https://login.microsoftonline.com/common/oauth2/v2.0/authorize"
                + $"?client_id={_configuration["AzureAd:ClientId"]}"
                + "&response_type=code"
                + $"&redirect_uri={_configuration["AzureAd:RedirectUri"]}"
                + "&response_mode=query"
                + $"&scope={getScopes(source)}"
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
            Logger.Log("Hitting the exchange endpoint");

            string stateJson = _dataProtector.Unprotect(state);
            DTO.AuthState authState = JsonSerializer.Deserialize<DTO.AuthState>(stateJson)!;

            //check if state is issued more than five minutes ago
            if (authState.issuedAt.AddMinutes(MAX_STATE_AGE_MINUTES) < DateTimeOffset.UtcNow)
            {
                Logger.Log("authorization request was expired");
                return Unauthorized("Authorization request has expired");
            }

            Models.User? user = _context.Users.Where(u => u.Id == authState.userId).FirstOrDefault();

            if (user == null)
            {
                Logger.Log("State was invalid", Logger.LogLevel.Warning);
                return Unauthorized("Invalid state");
            }

            HttpResponseMessage response = await _httpClient.PostAsync($"https://login.microsoftonline.com/common/oauth2/v2.0/token",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["client_id"] = _configuration["AzureAd:ClientId"]!,
                    ["client_secret"] = _configuration["AzureAd:ClientSecret"]!,
                    ["grant_type"] = "authorization_code",
                    ["code"] = code,
                    ["redirect_uri"] = _configuration["AzureAd:RedirectUri"]!,
                    ["scope"] = getScopes(authState.sourceRequest)
                }));

            string contentData = await response.Content.ReadAsStringAsync();
            Logger.Log(contentData, Logger.LogLevel.Warning);
            JsonDocument json = JsonDocument.Parse(contentData);

            string? graphAccessToken = json.RootElement.GetProperty("access_token").GetString();
            string? graphRefreshToken = json.RootElement.GetProperty("refresh_token").GetString();

            GraphClient graphClient = _graphClientProvider.Create(new Models.Registration()
            {
                AccessToken = graphAccessToken!,
                RefreshToken = graphRefreshToken!,
            }, _context);

            Graph.User me = await graphClient.Me();

            if (authState.sourceRequest)
            {
                user.OneDriveRegistration.Enabled = true;
                user.OneDriveRegistration.LinkedAccount = me.UserPrincipalName;
                user.OneDriveRegistration.AccessToken = graphAccessToken!;
                user.OneDriveRegistration.RefreshToken = graphRefreshToken!;
            }
            else
            {
                user.CalendarRegistration.Enabled = true;
                user.CalendarRegistration.LinkedAccount = me.UserPrincipalName;
                user.CalendarRegistration.AccessToken = graphAccessToken!;
                user.CalendarRegistration.RefreshToken = graphRefreshToken!;
            }


            _context.SaveChanges();

            string content = """
                <html>
                    <body>
                        <p>You have successfully registered, you can now close this page</p>
                        <script>
                            if (window.opener) {
                                window.opener.postMessage(
                                { success: true },
                                    "__DASHBOARD_URL__"
                                );
                            }
                                window.close();
                        </script>
                    </body>
                </html>
            """;

            content = content.Replace("__DASHBOARD_URL__", _configuration["postRegistrationMessage"]);
            return Content(content, "text/html");
        }

        /// <summary>
        /// Removes a registration from the user
        /// </summary>
        /// <param name="unregisterRequest">True if it is the filesource registration, false if it is the calendar registration</param>
        /// <returns>A status and message indicating whether the unregistration was successful</returns>
        [Authorize]
        [HttpPost("unregister")]
        public async Task<IActionResult> Unregister([FromBody] UnregisterRequest unregisterRequest)
        {
            (bool isValidRequest, Models.User user, IActionResult requestError) = await Util.Auth.ValidateRequest(this, _context);
            if (!isValidRequest) return requestError;

            if (unregisterRequest.Source)
            {
                if (user.OneDriveRegistration.LinkedAccount == null)
                {
                    return Conflict(new
                    {
                        Success = false,
                        Message = "User has no OneDrive registration"
                    });
                }

                user.OneDriveRegistration.LinkedAccount = "";
                user.OneDriveRegistration.Enabled = false;
                user.OneDriveRegistration.AccessToken = "";
                user.OneDriveRegistration.RefreshToken = "";

                _context.SaveChanges();

                return Ok(new
                {
                    Success = true,
                    Message = "OneDrive registration was removed"
                });
            } else
            {
                {
                if (user.CalendarRegistration.LinkedAccount == null)
                {
                    return Conflict(new
                    {
                        Success = false,
                        Message = "User has no Calendar registration"
                    });
                }

                user.CalendarRegistration.LinkedAccount = "";
                user.CalendarRegistration.Enabled = false;
                user.CalendarRegistration.AccessToken = "";
                user.CalendarRegistration.RefreshToken = "";

                _context.SaveChanges();

                return Ok(new
                {
                    Success = true,
                    Message = "Calendar registration was removed"
                });
            }
            }
        }


        string getScopes(bool source)
        {
            string scopes = source
                ? "offline_access User.Read Files.Read"
            : "offline_access User.Read Calendars.ReadWrite";

            return scopes;
        }
    }
}