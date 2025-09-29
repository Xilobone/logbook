using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Logbook.Controllers
{
    /// <summary>
    /// Serves as a simple monitor page for the service, only page not restricted by authentication
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class StatusController : ControllerBase
    {   
        /// <summary>
        /// Gets the status of the service (always running whenever it is able to return something)
        /// </summary>
        /// <returns>Status indicating that the service is working properly</returns>
        [HttpGet]
        public IActionResult GetStatus()
        {
            return Ok("Service is working properly");
        }
    }
}