using Microsoft.AspNetCore.Mvc;

namespace Logbook.Controllers
{
    /// <summary>
    /// Api endpoint used to obtain information about macros
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MacrosController : ControllerBase
    {
        /// <summary>
        /// Gets a json of all available macros, with their description
        /// </summary>
        /// <returns>A json of all macros</returns>
        [HttpGet]
        public IActionResult GetMacros()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "macros.json");

            if (!System.IO.File.Exists(filePath))
            {
                Logger.Log("Macro file not found", Logger.LogLevel.Error);
                return NotFound("Macro file not found");
            }

            var json = System.IO.File.ReadAllText(filePath);

            return Content(json, "application/json");
        }
    }
}