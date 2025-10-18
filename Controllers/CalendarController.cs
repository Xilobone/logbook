using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph.Models;
using Microsoft.Graph.Sites.GetAllSites;
using Logbook.Calendar;
using Microsoft.Graph;
using Microsoft.Extensions.Options;
using Logbook.Data;


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
        readonly LogbookDBContext _context;

        /// <summary>
        /// Creates a new calendar controller
        /// </summary>
        /// <param name="config">The calendar configuration to use for this controller</param>
        /// <param name="context">The database context to use</param>
        public CalendarController(IOptions<CalendarConfig> config, LogbookDBContext context)
        {
            _config = config.Value;
            _context = context;
        }

        /// <summary>
        /// Creates a calendar based on the contents in the provided file
        /// </summary>
        /// <returns>A summary of the performed task</returns>
        /// <exception cref="InvalidDataException">Thrown if no calendar was able to be created</exception>
        [HttpPost("create")]
        public async Task<IActionResult> CreateCalendar()
        {
            //exchange token for a graph client
            var incomingToken = await HttpContext.GetTokenAsync("access_token");
            GraphServiceClient graphClient = GraphClient.GetByAccessCode(incomingToken);

            Guid userId = GraphClient.GetUserEntraId(incomingToken!);
            Models.User user = _context.Users.Where(user => user.EntraId == userId).First();

            int eventCount = 0;
            foreach (Models.Group group in user.Groups)
            {
                //download file
                Drive? driveItem = await graphClient.Me.Drive.WithUrl($"https://graph.microsoft.com/v1.0/me/drive/root:{group.FilePath}").GetAsync();
                string? downloadUrl = (string)driveItem!.AdditionalData["@microsoft.graph.downloadUrl"];

                using var httpClient = new HttpClient();
                var fileBytes = await httpClient.GetByteArrayAsync(downloadUrl);

                CalendarManager calendarManager = new CalendarManager(group, graphClient);

                using var stream = new MemoryStream(fileBytes);
                List<Models.Event> events = calendarManager.CreateEventsFromStream(stream);

                string? calendarId = await calendarManager.GetOrCreateCalendar(user.CalendarName);

                if (calendarId == null) throw new InvalidDataException("No calendar id was returned");

                await calendarManager.UpdateCalendar(calendarId, events);

                eventCount += events.Count;
            }

            return Ok(eventCount);
        }
    }
}