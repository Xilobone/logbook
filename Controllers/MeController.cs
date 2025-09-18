using logbook;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Identity.Client;

namespace Logbook.Controllers
{
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

        [HttpGet("files")]
        public async Task<IActionResult> GetMyFiles()
        {
            var incomingToken = await HttpContext.GetTokenAsync("access_token");
            var graphClient = GraphClient.GetByAccessCode(incomingToken);

            // Get the default drive for the user
            var drive = await graphClient.Me.Drive.GetAsync();

            // Get the root folder DriveItem
            var rootFolder = await graphClient.Drives[drive.Id].Root.GetAsync();

            // List all items in the root folder using the Children request builder
            var childrenRequestBuilder = graphClient.Drives[drive.Id].Items[rootFolder.Id].Children;

            var rootItems = await childrenRequestBuilder.GetAsync();

            // Filter files only
            foreach (var item in rootItems.Value.Where(i => i.File != null))
            {
                Console.WriteLine($"File: {item.Name}, Id: {item.Id}");
            }

             return Ok(rootItems.Value);
        }

           
    }
}