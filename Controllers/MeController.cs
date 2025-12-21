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
        /// Retuns some basic info about the authenticated user
        /// </summary>
        /// <returns>The users display name and principal name</returns>
        [HttpGet]
        public async Task<IActionResult> Me()
        {
            DTO.TokenCaller? caller = await Util.Auth.GetCallerByHttpContext(HttpContext);
            if (caller == null) return Unauthorized("No valid token was provided");

            User? user = Util.User.GetUserByCaller(caller, _context);
            if (user == null) return Forbid("User is not registered");

            GraphClient graphClient = _graphClientProvider.Create(user, _context);

            string me = await graphClient.Me();

            return Ok(me);
        }
    }
}