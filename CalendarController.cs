using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph.Sites.GetAllSites;

namespace Logbook.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CalendarController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetCalendar()
        {
            var incomingToken = await HttpContext.GetTokenAsync("access_token");
            var graphClient = GraphClient.GetByAccessCode(incomingToken);

            GetAllSitesGetResponse? response = await graphClient.Sites.GetAllSites.GetAsGetAllSitesGetResponseAsync();

            return Ok(response);

        }
    }
}