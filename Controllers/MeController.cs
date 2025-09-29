using Microsoft.AspNetCore.Authentication;
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
        /// <summary>
        /// Retuns some basic info about the authenticated user
        /// </summary>
        /// <returns>The users display name and principal name</returns>
        [HttpGet]
        public async Task<IActionResult> GetMe()
        {
            var incomingToken = await HttpContext.GetTokenAsync("access_token");
            var graphClient = GraphClient.GetByAccessCode(incomingToken);

            var me = await graphClient.Me.GetAsync();

            return Ok(new { me!.DisplayName, me.UserPrincipalName });
        }  
    }
}