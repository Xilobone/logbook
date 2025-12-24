// using System.ComponentModel.DataAnnotations;
// using Logbook.Data;
// using Logbook.Models;
// using Microsoft.AspNetCore.Authentication;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;

using Logbook.Data;
using Logbook.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logbook.Controllers
{
    /// <summary>
    /// Api endpoint for reading and updating the configuration, only returns the configuration
    /// the user is allowed to see and edit
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ConfigController : ControllerBase
    {
        readonly LogbookDBContext _context;
        /// <summary>
        /// Creates a new config controller
        /// </summary>
        /// <param name="context">The database context to use</param>
        public ConfigController(LogbookDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets the users configuration
        /// </summary>
        /// <returns>The users configuration</returns>
        [HttpGet("personal")]
        public async Task<IActionResult> GetConfig()
        {
            DTO.TokenCaller? caller = await Util.Auth.GetCallerByHttpContext(HttpContext);
            if (caller == null) return Unauthorized("No valid token was provided");

            User? user = Util.User.GetUserByCaller(caller, _context);
            if (user == null) return Forbid("User is not registered");

            var config = new
            {
                user.DisplayName,
                user.Enabled,
                user.CalendarName,
            };

            return Ok(config);
        }

        /// <summary>
        /// Updates the users personal configuration
        /// </summary>
        /// <param name="config">The users updated configuration</param>
        /// <returns>204 no content</returns>
        [HttpPost("personal")]
        public async Task<IActionResult> SetConfig([FromBody] DTO.PersonalConfig config)
        {
            DTO.TokenCaller? caller = await Util.Auth.GetCallerByHttpContext(HttpContext);
            if (caller == null) return Unauthorized("No valid token was provided");

            User? user = Util.User.GetUserByCaller(caller, _context);
            if (user == null) return Forbid("User is not registered");

            if (config.enabled != null) user.Enabled = (bool)config.enabled;
            if (!string.IsNullOrEmpty(config.displayName)) user.DisplayName = config.displayName!;
            if (!string.IsNullOrEmpty(config.calendarName)) user.CalendarName = config.calendarName!;

            _context.SaveChanges();
            return NoContent();
        }
    }
}
// {
//     /// <summary>
//     /// Api endpoint for reading and updating the configuration, only returns the configuration
//     /// the user is allowed to see and edit
//     /// </summary>
//     [Authorize]
//     [ApiController]
//     [Route("api/[controller]")]
//     public class ConfigController : ControllerBase
//     {
//         readonly LogbookDBContext _context;
//         readonly GraphClient _graphClient;

//         /// <summary>
//         /// Creates a new config controller
//         /// </summary>
//         /// <param name="context">The database context to use</param>
//         /// <param name="graphClient">The graph client to use</param>
//         public ConfigController(LogbookDBContext context, GraphClient graphClient)
//         {
//             _context = context;
//             _graphClient = graphClient;
//         }
//         /// <summary>
//         /// Gets the users groups configuration
//         /// </summary>
//         /// <returns>The configuration of the groups the user is part of</returns>
//         [HttpGet("groups")]
//         public async Task<IActionResult> GetGroupsConfig()
//         {
//             string? incomingToken = await HttpContext.GetTokenAsync("access_token");

//             if (incomingToken == null) return Unauthorized("No token provided");

//             User? user = await Util.User.GetOrCreate(HttpContext, _context, _graphClient);

//             if (user == null)
//             {
//                 Logger.Log("User is null");
//                 return Problem("User doesnt exist");
//             }

//             var groups = user.Groups.Select(group => new
//             {
//                 group.Id,
//                 group.Name,
//                 group.StartTime,
//                 group.EndTime,
//                 group.TimeZone,
//                 group.FilePath

//             });

//             return Ok(groups);
//         }

//         /// <summary>
//         /// Gets the users personal configuration
//         /// </summary>
//         /// <returns>The users personal configuration</returns>
//         [HttpGet("me")]
//         public async Task<IActionResult> GetMeConfig()
//         {
//             string? incomingToken = await HttpContext.GetTokenAsync("access_token");

//             if (incomingToken == null) return Unauthorized("No token provided");

//             User? user = await Util.User.GetOrCreate(HttpContext, _context, _graphClient);

//             if (user == null)
//             {
//                 Logger.Log("User is null");
//                 return Problem("User doesnt exist");
//             }

//             var data = new
//             {
//                 Id = user.Id,
//                 user.CalendarName,
//                 user.Enabled
//             };

//             return Ok(data);

//         }

//         /// <summary>
//         /// Updates the configuration of a specified group
//         /// </summary>
//         /// <param name="param">The parameters used to update</param>
//         /// <returns>A 200 code if the status was updated</returns>
//         [HttpPost("group/update")]
//         public IActionResult UpdateGroupConfig([FromBody] UpdateGroupConfigParams param)
//         {
//             //TODO: add validation that user is actually in the group that it requested to update
//             Logger.Log("Update config endpoint called");
//             if (!ModelState.IsValid)
//             {
//                 Logger.Log("Invalid parameters");
//                 return BadRequest(ModelState);
//             }

//             Group? group = _context.Groups.Where(group => group.Id == param.Id).FirstOrDefault();

//             if (group == null) return NotFound($"No group with id {param.Id} was found");

//             Logger.Log($"group with id {param.Id} was found, named {group.Name}");
//             Logger.Log($"New name: {param.Name}");
//             //update values
//             if (!string.IsNullOrEmpty(param.Name)) group.Name = param.Name;
//             if (!string.IsNullOrEmpty(param.FilePath)) group.FilePath = param.FilePath;
//             if (!string.IsNullOrEmpty(param.TimeZone)) group.TimeZone = param.TimeZone;
//             if (param.StartTime.HasValue) group.StartTime = param.StartTime.Value;
//             if (param.EndTime.HasValue) group.EndTime = param.EndTime.Value;

//             _context.SaveChanges();

//             return Ok("data updated");
//         }

//         /// <summary>
//         /// Updates the configuration of a specified group
//         /// </summary>
//         /// <param name="param">The parameters used to update</param>
//         /// <returns>A 200 code if the status was updated</returns>
//         [HttpPost("me/update")]
//         public async Task<IActionResult> UpdateMeConfig([FromBody] UpdateMeConfigParams param)
//         {   
//             Logger.Log("Update config endpoint called");
//             if (!ModelState.IsValid)
//             {
//                 Logger.Log("Invalid parameters");
//                 return BadRequest(ModelState);
//             }

//             string? incomingToken = await HttpContext.GetTokenAsync("access_token");
//             if (incomingToken == null) return Unauthorized("No token provided");

//             User? user = await Util.User.Get(HttpContext, _context, _graphClient);

//             if (user == null) return NotFound("User not registered");

//             if (!string.IsNullOrEmpty(param.CalendarName)) user.CalendarName = param.CalendarName;
//             if (param.Enabled != null) user.Enabled = (bool) param.Enabled;

//             _context.SaveChanges();

//             return Ok("data updated");
//         }

//         /// <summary>
//         /// Parameters used for the update group endpoint
//         /// </summary>
//         public class UpdateGroupConfigParams
//         {
//             /// <summary>
//             /// The id of the group that is updated
//             /// </summary>
//             [Required]
//             public Guid Id { get; set; }

//             /// <summary>
//             /// The updated path of the schedule file
//             /// </summary>
//             public string? FilePath { get; set; }

//             /// <summary>
//             /// The updated name of the group
//             /// </summary>
//             public string? Name { get; set; }

//             /// <summary>
//             /// The updated default start time
//             /// </summary>
//             public TimeOnly? StartTime { get; set; }

//             /// <summary>
//             /// The updated default end time
//             /// </summary>
//             public TimeOnly? EndTime { get; set; }

//             /// <summary>
//             /// The updated timezone
//             /// </summary>
//             public string? TimeZone { get; set; }
//         }

//         /// <summary>
//         /// Parameters used for the update me endpoint
//         /// </summary>
//         public class UpdateMeConfigParams
//         {
//             /// <summary>
//             /// The updated calendar name
//             /// </summary>
//             public string? CalendarName { get; set; }

//             /// <summary>
//             /// The updated enabled status
//             /// </summary>
//             public bool? Enabled { get; set; }
//         }
//     }
// }