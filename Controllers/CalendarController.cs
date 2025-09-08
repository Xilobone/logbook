using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph.Models;
using Microsoft.Graph.Sites.GetAllSites;
using Logbook.Calendar;
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

        [HttpPost("create")]
        public async Task<IActionResult> CreateCalendar([FromBody] CreateCalendarParams param)
        {   
            //exchange token for a graph client
            var incomingToken = await HttpContext.GetTokenAsync("access_token");
            var graphClient = GraphClient.GetByAccessCode(incomingToken);

            //download file
            Drive? driveItem = await graphClient.Me.Drive.WithUrl($"https://graph.microsoft.com/v1.0/me/drive/root:{param.path}").GetAsync();
            string? downloadUrl = (string)driveItem!.AdditionalData["@microsoft.graph.downloadUrl"];

            using var httpClient = new HttpClient();
            var fileBytes = await httpClient.GetByteArrayAsync(downloadUrl);

            // Save to disk
            await System.IO.File.WriteAllBytesAsync("files/report.xlsx", fileBytes);
            Console.WriteLine("File downloaded successfully.");


            using var stream = new MemoryStream(fileBytes);
            List<CalendarEvent> events = CalendarEvent.CreateFromStream(stream);

            foreach (CalendarEvent e in events)
            {
                Console.WriteLine($"({e.StartTime} - {e.EndTime}) {e.Title}");
            }
            return Ok(events.Count);
        }

        public class CreateCalendarParams
        {
            public string path { get; set; } = string.Empty;

        }

    }
}