using Logbook.Data;
using Logbook.Graph;
using Logbook.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logbook.Controllers
{
    /// <summary>
    /// Endpoint for obtaining info about the user
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MeController : ControllerBase
    {
        readonly LogbookDBContext _context;
        readonly GraphClientProvider _graphClientProvider;

        /// <summary>
        /// Create a new controller for the Me endpoint
        /// </summary>
        public MeController(LogbookDBContext context, GraphClientProvider graphClientProvider)
        {
            _context = context;
            _graphClientProvider = graphClientProvider;
        }
        /// <summary>
        /// Retuns info about the user, the only endpoint that unregistered users are allowed to called
        /// (except registration endpoints). If an unregistered user calls this endpoint they will get their
        /// displayname returned and a false boolean indicating their registration status
        /// </summary>
        /// <returns>The users values and configuration</returns>
        [HttpGet]
        public async Task<IActionResult> GetMe()
        {
            (bool isValidRequest, User user, IActionResult error) = await Util.Auth.ValidateRequest(this, _context);
            //we allow the error to be forbid in this specific case, unregistered users are allowed
            if (!isValidRequest && error is UnauthorizedResult) return error;

            DTO.TokenCaller? caller = await Util.Auth.GetCallerByHttpContext(HttpContext);
            if (caller == null) return Unauthorized("No valid token was provided");

            if (user == null)
            {
                return Ok(new
                {
                    caller.DisplayName,
                    Registered = false
                });
            }

            return Ok(new
            {
                user.Id,
                user.DisplayName,
                user.Enabled,
                user.CalendarName,
                user.CanBeSource,
                user.Alias,
                user.AliasMatchingType,
            });
        }

        /// <summary>
        /// Updates the users personal configuration
        /// </summary>
        /// <param name="config">The users updated configuration</param>
        /// <returns>A message indicating the config was updated</returns>
        [HttpPost()]
        public async Task<IActionResult> SetPersonalConfig([FromBody] DTO.Me config)
        {
            (bool isValidRequest, User user, IActionResult error) = await Util.Auth.ValidateRequest(this, _context);
            if (!isValidRequest) return error;

            if (config.Enabled != null) user.Enabled = (bool)config.Enabled;
            if (!string.IsNullOrEmpty(config.DisplayName)) user.DisplayName = config.DisplayName!;
            if (!string.IsNullOrEmpty(config.CalendarName)) user.CalendarName = config.CalendarName!;
            if (!string.IsNullOrEmpty(config.Alias)) user.Alias = config.Alias!;
            if (config.AliasMatchingType != null) user.AliasMatchingType = (User.AliasMatching) config.AliasMatchingType;

            _context.SaveChanges();
            return Ok(new
            {
                Message = "User configuration was successfully changed",
                Success = true,
                UserId = user.Id
            });
        }
    }
}