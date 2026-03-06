using Microsoft.AspNetCore.Mvc;

namespace Logbook.Controllers
{
    /// <summary>
    /// Api controller for getting the available timezones
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TimezonesController : ControllerBase
    {
        /// <summary>
        /// Gets an array of all available timezones
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetTimezones()
        {
            string[] timezones = TimeZoneInfo.GetSystemTimeZones().Select(tz => tz.Id).Order().ToArray();

            return Ok(new
            {
                timezones
            });

        }
    }
}