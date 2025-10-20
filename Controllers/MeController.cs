using Logbook.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph.Models;

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
            Logger.Log("Called Me endpoint");

            var incomingToken = await HttpContext.GetTokenAsync("access_token");
            var graphClient = GraphClient.GetByAccessCode(incomingToken);
            Logger.Log("Created graph client");

            User? me = await graphClient.Me.GetAsync();
            DirectoryObjectCollectionResponse? memberOf = await graphClient.Me.MemberOf.GetAsync();

            return Ok(new
            {
                me!.DisplayName,
                me!.UserPrincipalName,
                memberOf!.Value
            });
        }
    }
}