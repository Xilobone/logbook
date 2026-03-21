using Logbook.Data;
using Logbook.Models;
using Logbook.Util;
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

        /// <summary>
        /// Create a new controller for the Me endpoint
        /// </summary>
        public MeController(LogbookDBContext context)
        {
            _context = context;
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
            (bool isValidRequest, Models.User user, IActionResult error) = await Util.Auth.ValidateRequest(this, _context);
            //we allow the error to be forbid in this specific case, unregistered users are allowed
            if (!isValidRequest && error is UnauthorizedResult) return error;

            DTO.TokenCaller? caller = await Util.Auth.GetCallerByHttpContext(HttpContext);
            if (caller == null) return Unauthorized("No valid token was provided");

            if (user == null)
            {
                //User doesnt exist yet
                user = new Models.User()
                {
                    Id = caller.Id,
                    Username = caller.UserPrincipalName,
                    DisplayName = caller.DisplayName,
                    CalendarRegistration = new Registration(),
                    OneDriveRegistration = new Registration(),
                    // Enabled = false,
                    // CanBeSource = false,
                    // HasCalendarLinked = false,
                };

                _context.Users.Add(user);
                _context.SaveChanges();
            }

            return Ok(ModelConverter.ToDTO.User(user));
        }

        /// <summary>
        /// Updates the users personal configuration
        /// </summary>
        /// <param name="config">The users updated configuration</param>
        /// <returns>A message indicating the config was updated</returns>
        [HttpPost()]
        public async Task<IActionResult> SetPersonalConfig([FromBody] DTO.User config)
        {
            (bool isValidRequest, Models.User user, IActionResult error) = await Auth.ValidateRequest(this, _context);
            if (!isValidRequest) return error;

            // if (config.Enabled != null) user.Enabled = (bool)config.Enabled;
            if (!string.IsNullOrEmpty(config.DisplayName)) user.DisplayName = config.DisplayName!;
            if (!string.IsNullOrEmpty(config.CalendarName)) user.CalendarName = config.CalendarName!;
            if (!string.IsNullOrEmpty(config.Alias)) user.Alias = config.Alias!;
            if (config.AliasMatchingType != null) user.AliasMatchingType = (Models.User.AliasMatching)config.AliasMatchingType;
            if (config.CalendarRegistration != null)
            {
                if (config.CalendarRegistration.Enabled != null) user.CalendarRegistration.Enabled = (bool)config.CalendarRegistration.Enabled;
            }

            if (config.OneDriveRegistration != null)
            {
                if (config.OneDriveRegistration.Enabled != null) user.OneDriveRegistration.Enabled = (bool)config.OneDriveRegistration.Enabled;
            }
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