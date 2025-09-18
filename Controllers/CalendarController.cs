using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph.Models;
using Microsoft.Graph.Sites.GetAllSites;
using Logbook.Calendar;
using Microsoft.Graph;
using Logbook.Models;
using System.Text.Json;

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
            var graphClient = ConfidentialClient.GetByAccessCode(incomingToken);

            GetAllSitesGetResponse? response = await graphClient.Sites.GetAllSites.GetAsGetAllSitesGetResponseAsync();

            return Ok(response);

        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateCalendar([FromBody] CreateCalendarParams param)
        {
            //exchange token for a graph client
            var incomingToken = await HttpContext.GetTokenAsync("access_token");
            GraphServiceClient graphClient = ConfidentialClient.GetByAccessCode(incomingToken);

            //download file
            Drive? driveItem = await graphClient.Me.Drive.WithUrl($"https://graph.microsoft.com/v1.0/me/drive/root:{param.path}").GetAsync();
            string? downloadUrl = (string)driveItem!.AdditionalData["@microsoft.graph.downloadUrl"];

            using var httpClient = new HttpClient();
            var fileBytes = await httpClient.GetByteArrayAsync(downloadUrl);

            // Save to disk
            await System.IO.File.WriteAllBytesAsync("files/report.xlsx", fileBytes);
            Console.WriteLine("File downloaded successfully.");


            using var stream = new MemoryStream(fileBytes);
            List<Models.Event> events = CalendarManager.CreateEventsFromStream(stream);

            CalendarManager calendarManager = new CalendarManager(graphClient);
            string? calendarId = await calendarManager.GetOrCreateCalendar("Planning");

            if (calendarId == null) throw new InvalidDataException("No calendar id was returned");

            await calendarManager.UpdateCalendar(calendarId, events);

            return Ok(events.Count);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            //exchange token for a graph client
            var incomingToken = await HttpContext.GetTokenAsync("access_token");
            GraphServiceClient graphClient = ConfidentialClient.GetByAccessCode(incomingToken);

            CalendarManager calendarManager = new CalendarManager(graphClient);
            string? calendarId = await calendarManager.GetOrCreateCalendar("Planning");

            if (calendarId == null) throw new InvalidDataException("No calendar id was returned");

            Calendar.Calendar calendar = new Logbook.Calendar.Calendar(graphClient, calendarId);

            List<Models.Event> events = await calendar.GetAllEvents();

            var data = new
            {
                Count = events.Count,
                Data = events,
            };
            // string json = JsonSerializer.Serialize(data);
            return Ok(data);
        }

        public class CreateCalendarParams
        {
            public string path { get; set; } = string.Empty;

        }

    }
}