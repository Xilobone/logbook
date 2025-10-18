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
                group.Name,
                group.StartTime,
                group.EndTime,
                group.FilePath

            });

            return Ok(groups);
        }
    }
}