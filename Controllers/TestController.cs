using Logbook.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;

namespace Logbook.Controllers
{
    /// <summary>
    /// Api endpoint for testing purposes
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {   
        readonly LogbookDBContext _context;

        /// <summary>
        /// Creates a new test controller
        /// </summary>
        /// <param name="context">The database context to use</param>
        public TestController(LogbookDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Performs a test
        /// </summary>
        /// <returns>200 ok if the test was successful</returns>
        [HttpGet]
        public async Task<IActionResult> Test()
        {
            var incomingToken = await HttpContext.GetTokenAsync("access_token");
            GraphServiceClient graphClient = GraphClient.GetByAccessCode(incomingToken);
            
            EventUpdater eventUpdater = new EventUpdater(graphClient, _context);
            await eventUpdater.Update();

            return Ok();
        }
    }
}