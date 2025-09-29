using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph.Models;
using Microsoft.Graph.Sites.GetAllSites;
using Logbook.Calendar;
using Microsoft.Graph;
using Microsoft.Extensions.Options;


namespace Logbook.Controllers
{   
    /// <summary>
    /// Handles api requests about the users calendar
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CalendarController : ControllerBase
    {
        readonly CalendarConfig _config;

        /// <summary>
        /// Creates a new calendar controller
        /// </summary>
        /// <param name="config">The calendar configuration to use for this controller</param>
        public CalendarController(IOptions<CalendarConfig> config)
        {
            _config = config.Value;
        }

        /// <summary>
        /// Creates a calendar based on the contents in the provided file
        /// </summary>
        /// <param name="param">The parameters passed to the function, include the source file and an optional custom name</param>
        /// <returns>A summary of the performed task</returns>
        /// <exception cref="InvalidDataException">Thrown if no calendar was able to be created</exception>
        [HttpPost("create")]
        public async Task<IActionResult> CreateCalendar([FromBody] CreateCalendarParams param)
        {
            //exchange token for a graph client
            var incomingToken = await HttpContext.GetTokenAsync("access_token");
            GraphServiceClient graphClient = GraphClient.GetByAccessCode(incomingToken);

            //download file
            Drive? driveItem = await graphClient.Me.Drive.WithUrl($"https://graph.microsoft.com/v1.0/me/drive/root:{param.path}").GetAsync();
            string? downloadUrl = (string)driveItem!.AdditionalData["@microsoft.graph.downloadUrl"];

            using var httpClient = new HttpClient();
            var fileBytes = await httpClient.GetByteArrayAsync(downloadUrl);

            CalendarManager calendarManager = new CalendarManager(_config, graphClient);

            using var stream = new MemoryStream(fileBytes);
            List<Models.Event> events = calendarManager.CreateEventsFromStream(stream);

            string? calendarId = await calendarManager.GetOrCreateCalendar(param.name);

            if (calendarId == null) throw new InvalidDataException("No calendar id was returned");

            await calendarManager.UpdateCalendar(calendarId, events);

            return Ok(events.Count);
        }

        /// <summary>
        /// Gets a list of all events currently in the calendar
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidDataException"></exception>
        [HttpGet("all")]
        public async Task<IActionResult> GetAll([FromQuery] string name)
        {
            //exchange token for a graph client
            var incomingToken = await HttpContext.GetTokenAsync("access_token");
            GraphServiceClient graphClient = GraphClient.GetByAccessCode(incomingToken);

            CalendarManager calendarManager = new CalendarManager(_config,graphClient);
            string? calendarId = await calendarManager.GetOrCreateCalendar(name);

            if (calendarId == null) throw new InvalidDataException("No calendar id was returned");

            Calendar.Calendar calendar = new Logbook.Calendar.Calendar(graphClient, calendarId);

            List<Models.Event> events = await calendar.GetAllEvents();

            var data = new
            {
                events.Count,
                Data = events,
            };
            return Ok(data);
        }

        /// <summary>
        /// Parameters that can be passed to the create endpoint
        /// </summary>
        public class CreateCalendarParams
        {
            /// <summary>
            /// The path of the source file to create a parameter of
            /// </summary>
            public string path { get; set; } = string.Empty;

            /// <summary>
            /// The name of the calendar to create, defaults to 'Planning'
            /// </summary>
            public string name { get; set; } = "Planning";

        }

    }
}