using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Logbook.Data;

namespace Logbook.Graph
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
        /// <summary>
        /// Object containing all graph methods regaring calendars
        /// </summary>
        public readonly GraphCalendarClient Calendars;
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

            Calendars = new GraphCalendarClient(this, _user);
        }

        /// <summary>
        /// Gets information about the user
        /// </summary>
        /// <returns>Data about the user</returns>
        public async Task<string> Me()
        {
            return await MakeGraphRequestGet("me");
        }

        /// <summary>
        /// Gets the content of a users file in Onedrive
        /// </summary>
        /// <param name="filePath">The path of the file to get</param>
        /// <returns>The content of the file, as a byte array</returns>
        public async Task<byte[]> GetOnedriveFile(string filePath)
        {
            string response = await MakeGraphRequestGet($"me/drive/root:{filePath}");
            Graph.DriveItem driveItem = JsonSerializer.Deserialize<Graph.DriveItem>(response)!;

            return await _httpClient.GetByteArrayAsync(driveItem.DownloadUrl);
        }

        /// <summary>
        /// Makes a GET request to the graph api
        /// </summary>
        /// <param name="endpoint">The endpoint to request</param>
        /// <param name="prefixGraph">Whether to prefix the graph url to the endpoint, default true</param>
        /// <returns>A string containing the graph response as json</returns>
        public async Task<string> MakeGraphRequestGet(string endpoint, bool prefixGraph = true)
        {
            string url = prefixGraph ? $"{_config["AzureAD:GraphUrl"]}{endpoint}" : endpoint;
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            //Token expired
            if ((int)response.StatusCode == StatusCodes.Status401Unauthorized)
            {
                string newAccessToken = await RefreshToken();

                //Set new token as header and try the endpoint again
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", newAccessToken);
                response = await _httpClient.GetAsync(url);
            }

            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Makes a POST request to the graph api
        /// </summary>
        /// <param name="endpoint">The endpoint to request</param>
        /// <param name="body">The request body as a json string</param>
        /// <param name="prefixGraph">Whether to prefix the graph url to the endpoint, default true</param>
        /// <returns>A string containing the graph response as json</returns>
        public async Task<string> MakeGraphRequestPost(string endpoint, string body, bool prefixGraph = true)
        {
            string url = prefixGraph ? $"{_config["AzureAD:GraphUrl"]}{endpoint}" : endpoint;

            using var content = new StringContent(body, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync(url, content);

            //Token expired
            if ((int)response.StatusCode == StatusCodes.Status401Unauthorized)
            {
                string newAccessToken = await RefreshToken();

                //Set new token as header and try the endpoint again
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", newAccessToken);
                response = await _httpClient.PostAsync(url, content);
            }

            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Makes a DELETE request to the graph api
        /// </summary>
        /// <param name="endpoint">The endpoint to request</param>
        /// <param name="prefixGraph">Whether to prefix the graph url to the endpoint, default true</param>
        /// <returns>A string containing the graph response as json</returns>
        public async Task<string> MakeGraphRequestDelete(string endpoint, bool prefixGraph = true)
        {
            string url = prefixGraph ? $"{_config["AzureAD:GraphUrl"]}{endpoint}" : endpoint;

            HttpResponseMessage response = await _httpClient.DeleteAsync(url);

            //Token expired
            if ((int)response.StatusCode == StatusCodes.Status401Unauthorized)
            {
                string newAccessToken = await RefreshToken();

                //Set new token as header and try the endpoint again
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", newAccessToken);
                response = await _httpClient.DeleteAsync(url);
            }

            return await response.Content.ReadAsStringAsync();
        }

        async Task<string> RefreshToken()
        {
            Logger.Log("Token was expired, obtaining a new token from Graph");
            string url = $"https://login.microsoftonline.com/{_config["AzureAD:TenantID"]}/oauth2/v2.0/token";
            HttpRequestMessage refreshRequest = new HttpRequestMessage(HttpMethod.Post,url);

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
            return token.AccessToken;
        }


    }
}