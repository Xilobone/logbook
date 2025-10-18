using Logbook.Calendar;
using Logbook.Data;
using Logbook.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace Logbook.Controllers
{
    /// <summary>
    /// Api endpoint for refreshing the stored calendar data
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class RefreshController : ControllerBase
    {

        readonly CalendarConfig _config;
        readonly LogbookDBContext _context;

        /// <summary>
        /// Creates a new refresh controller
        /// </summary>
        /// <param name="context">The database context to use</param>
        /// <param name="options">The calendar configuration to use</param>
        public RefreshController(LogbookDBContext context, IOptions<CalendarConfig> options)
        {
            _context = context;
            _config = options.Value;
        }

        // / <summary>
        // / Refreshes the stored calendar data
        // / </summary>
        // / <param name="param">The parameters of the refresh</param>
        // / <returns>A result summorizing what has happens</returns>
        // [HttpPost]
        // public async Task<IActionResult> RefreshData([FromBody] RefreshParams param)
        // {
        //     //exchange token for a graph client
        //     var incomingToken = await HttpContext.GetTokenAsync("access_token");
        //     GraphServiceClient graphClient = GraphClient.GetByAccessCode(incomingToken);
        //     Guid userId = GraphClient.GetUserEntraId(incomingToken!);
        //     Models.User user = _context.Users.Where(user => user.EntraId == userId).First();

        //     int eventCount = 0;
        //     foreach (Models.Group group in user.Groups)
        //     {
        //         //download file
        //         Drive? driveItem = await graphClient.Me.Drive.WithUrl($"https://graph.microsoft.com/v1.0/me/drive/root:{param.source}").GetAsync();
        //         string? downloadUrl = (string)driveItem!.AdditionalData["@microsoft.graph.downloadUrl"];

        //         using var httpClient = new HttpClient();
        //         var fileBytes = await httpClient.GetByteArrayAsync(downloadUrl);

        //         CalendarManager calendarManager = new CalendarManager(_config, graphClient);

        //         using var stream = new MemoryStream(fileBytes);
        //         List<Models.Event> events = calendarManager.CreateEventsFromStream(stream);

        //         //get group or create if it doesnt exist
        //         Models.Group? group = _context.Groups.Where(group => group.Name == param.group).FirstOrDefault();
        //         if (group == null)
        //         {
        //             group = new Models.Group()
        //             {
        //                 Name = param.group
        //             };
        //             _context.Groups.Add(group);
        //         }

        //         _context.SaveChanges();
        //         // string? calendarId = await calendarManager.GetOrCreateCalendar(param.name);

        //         // if (calendarId == null) throw new InvalidDataException("No calendar id was returned");

        //         // await calendarManager.UpdateCalendar(calendarId, events);
        //     }
        //         return Ok(events.Count);
        //     }
        }
    }