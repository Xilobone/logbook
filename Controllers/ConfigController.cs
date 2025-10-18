using System.ComponentModel.DataAnnotations;
using Logbook.Data;
using Logbook.Models;
using Microsoft.AspNetCore.Authentication;
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
        /// Gets the users personal configuration, as well as the config of any group they are part
        /// of
        /// </summary>
        /// <returns>The configuration</returns>
        [HttpGet]
        public async Task<IActionResult> GetConfig()
        {
            string? incomingToken = await HttpContext.GetTokenAsync("access_token");

            if (incomingToken == null) return Unauthorized("No token provided");

            User? user = await Util.User.GetOrCreate(HttpContext, _context);

            if (user == null)
            {
                Logger.Log("User is null");
                return Problem("User doesnt exist");
            }

            var groups = user.Groups.Select(group => new
            {
                group.Id,
                group.Name,
                group.StartTime,
                group.EndTime,
                group.TimeZone,
                group.FilePath

            });

            return Ok(groups);
        }

        /// <summary>
        /// Updates the configuration of a specified group
        /// </summary>
        /// <param name="param">The parameters used to update</param>
        /// <returns>A 200 code if the status was updated</returns>
        [HttpPost("update")]
        public IActionResult UpdateConfig([FromBody] UpdateConfigParams param)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Group? group = _context.Groups.Where(group => group.Id == param.Id).FirstOrDefault();

            if (group == null) return NotFound($"No group with id {param.Id} was found");

            //update values
            if (!string.IsNullOrEmpty(param.Name)) group.Name = param.Name;
            if (!string.IsNullOrEmpty(param.FilePath)) group.FilePath = param.FilePath;
            if (!string.IsNullOrEmpty(param.TimeZone)) group.TimeZone = param.TimeZone;
            if (param.StartTime.HasValue) group.StartTime = param.StartTime.Value;
            if (param.EndTime.HasValue) group.EndTime = param.EndTime.Value;

            _context.SaveChanges();

            return Ok("data updated");
        }

        /// <summary>
        /// Parameters used for the update endpoint
        /// </summary>
        public class UpdateConfigParams
        {
            /// <summary>
            /// The id of the group that is updated
            /// </summary>
            [Required]
            public Guid Id { get; set; }

            /// <summary>
            /// The updated path of the schedule file
            /// </summary>
            public string? FilePath { get; set; }

            /// <summary>
            /// The updated name of the group
            /// </summary>
            public string? Name { get; set; }

            /// <summary>
            /// The updated default start time
            /// </summary>
            public TimeOnly? StartTime { get; set; }

            /// <summary>
            /// The updated default end time
            /// </summary>
            public TimeOnly? EndTime { get; set; }

            /// <summary>
            /// The updated timezone
            /// </summary>
            public string? TimeZone { get; set; }
        }
    }
}