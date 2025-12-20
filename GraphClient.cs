using System.Net.Http.Headers;
using System.Text.Json;
using Logbook.Data;
using Logbook.Graph;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
// using Logbook.Models;

namespace Logbook
{
    /// <summary>
    /// Provides a way to create graph clients
    /// </summary>
    public class GraphClientProvider
    {
        readonly IConfiguration _config;

        /// <summary>
        /// Creates a new graph client provider
        /// </summary>
        /// <param name="config">The configuration to use</param>
        public GraphClientProvider(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Creates a new graph client
        /// </summary>
        /// <param name="user">The user that the client makes request on behalf of</param>
        /// <param name="context">The databasecontext to use</param>
        /// <returns>A graph client that can be used to make requests</returns>
        public GraphClient Create(Models.User user, LogbookDBContext context)
        {
            return new GraphClient(_config, user, context);
        }
    }

    /// <summary>
    /// A graph client that can make requests to microsoft graph on behalf of the user
    /// </summary>
    public class GraphClient
    {
        readonly IConfiguration _config;
        Models.User _user;
        LogbookDBContext _context;

        HttpClient _httpClient;

        /// <summary>
        /// Creates a new graph client
        /// </summary>
        /// <param name="config">The configuration to use</param>
        /// <param name="user">The user that the client makes request on behalf of</param>
        /// <param name="context">The database context to use to save new access and refresh tokens</param>
        public GraphClient(IConfiguration config, Models.User user, LogbookDBContext context)
        {
            _config = config;
            _user = user;
            _context = context;

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.AccessToken);
        }

        /// <summary>
        /// Gets information about the user
        /// </summary>
        /// <returns>Data about the user</returns>
        public async Task<string> Me()
        {
            return await MakeGraphRequest("me");
        }

        /// <summary>
        /// Gets the content of a users file in Onedrive
        /// </summary>
        /// <param name="filePath">The path of the file to get</param>
        /// <returns>The content of the file, as a byte array</returns>
        public async Task<byte[]> GetOnedriveFile(string filePath)
        {
            string response = await MakeGraphRequest($"me/drive/root:{filePath}");
            Graph.DriveItem driveItem = JsonSerializer.Deserialize<Graph.DriveItem>(response)!;

            return await _httpClient.GetByteArrayAsync(driveItem.DownloadUrl);
        }

        /// <summary>
        /// Gets the users calendar events
        /// </summary>
        /// <param name="calendarName">The name of the calendar to get</param>
        /// <returns>The users calendar events</returns>
        public async Task<List<Event>> GetCalendarEvents(string calendarName)
        {
            List<Event> events = new List<Event>();

            //get the id of the calendar first
            string response = await MakeGraphRequest($"me/calendars?$filter=name eq '{calendarName}'");
            QueryResponse<Calendar> calendars = JsonSerializer.Deserialize<QueryResponse<Calendar>>(response)!;

            if (calendars.Values.Count == 0)
            {
                Logger.Log($"Calendar named {calendarName} was not found for user with id {_user.Id}");
                return events;
            }

            string eventResponse = await MakeGraphRequest($"me/calendars/{calendars.Values[0].Id}/events");

            QueryResponse<Event> eventCollection = JsonSerializer.Deserialize<QueryResponse<Event>>(eventResponse)!;
            events.AddRange(eventCollection.Values);

            while (!string.IsNullOrEmpty(eventCollection.NextUrl))
            {
                eventResponse = await MakeGraphRequest(eventCollection.NextUrl, false);

                eventCollection = JsonSerializer.Deserialize<QueryResponse<Event>>(eventResponse)!;
                events.AddRange(eventCollection.Values);
            }
            return events;
        }

        async Task<string> MakeGraphRequest(string endpoint, bool prefixGraph = true)
        {
            string url = prefixGraph ? $"{_config["AzureAD:GraphUrl"]}{endpoint}" : endpoint;
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            //Token expired
            if ((int)response.StatusCode == StatusCodes.Status401Unauthorized)
            {
                Logger.Log("Token was expired, obtaining a new token from Graph");

                HttpRequestMessage refreshRequest = new HttpRequestMessage(HttpMethod.Post,
                    $"https://login.microsoftonline.com/{_config["AzureAD:TenantID"]}/oauth2/v2.0/token");

                refreshRequest.Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["client_id"] = _config["AzureAD:ClientId"]!,
                    ["client_secret"] = _config["AzureAD:ClientSecret"]!,
                    ["grant_type"] = "refresh_token",
                    ["refresh_token"] = _user.RefreshToken,
                    ["scope"] = "https://graph.microsoft.com/.default"
                });

                HttpResponseMessage refreshResponse = await _httpClient.SendAsync(refreshRequest);
                string refreshData = await refreshResponse.Content.ReadAsStringAsync();
                Graph.TokenResponse token = JsonSerializer.Deserialize<Graph.TokenResponse>(refreshData)!;

                _user.AccessToken = token.AccessToken;
                _user.RefreshToken = token.RefreshToken;

                _context.SaveChanges();

                //Set new token as header and try the endpoint again
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
                response = await _httpClient.GetAsync(endpoint);
            }

            return await response.Content.ReadAsStringAsync();
        }


    }
}