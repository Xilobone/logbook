using System.Reflection;
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
        readonly string _version;

        /// <summary>
        /// Creates a new status controller
        /// </summary>
        public StatusController()
        {
            _version = Assembly
                .GetEntryAssembly()!
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()!
                .InformationalVersion;
        }
        /// <summary>
        /// Gets the status of the service (always running whenever it is able to return something)
        /// and the version of the webapi
        /// </summary>
        /// <returns>Status indicating that the service is working properly</returns>
        [HttpGet]
        public IActionResult GetStatus()
        {
            return Ok(new
            {
                Message = "Service is working properly",
                Status = 0,
                Version = _version
            });
        }
    }
}