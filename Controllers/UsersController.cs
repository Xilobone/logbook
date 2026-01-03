using Logbook.Data;
using Logbook.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logbook.Controllers
{
    /// <summary>
    /// Endpoint for obtaining users
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        readonly LogbookDBContext _context;

        /// <summary>
        /// Create a new controller for the Users endpoint
        /// </summary>
        /// <param name="context">The database context to use</param>
        public UsersController(LogbookDBContext context) : base()
        {
            _context = context;
        }

        /// <summary>
        /// Gets a list of all enabled users
        /// </summary>
        /// <returns>A list of all enabled users</returns>
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            (bool isValidRequest, User user, IActionResult error) = await Util.Auth.ValidateRequest(this, _context);
            if (!isValidRequest) return error;

            var users = _context.Users.Where(u => u.Enabled).Select(u => new
            {
                u.Id,
                u.DisplayName
            });

            return Ok(new
            {
                users,
                Success = true
            });
        }
    }
}