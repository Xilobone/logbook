using Logbook.Data;
using Graph = Microsoft.Graph.Models;
using Microsoft.AspNetCore.Authentication;
using System.IdentityModel.Tokens.Jwt;
namespace Logbook.Util
{
    /// <summary>
    /// Class containing util functions regarding users
    /// </summary>
    public class User
    {
        /// <summary>
        /// Gets the stored user info that made the request, or gets the info from graph and inserts
        /// it into the database if it doesnt exist yet
        /// </summary>
        /// <param name="httpContext">The http request context</param>
        /// <param name="_context">The database context</param>
        /// <param name="graphClient">The graph client to use</param>
        /// <returns>The created or fetched user</returns>
        public static async Task<Models.User?> GetOrCreate(HttpContext httpContext, LogbookDBContext _context, GraphClient graphClient)
        {
            var incomingToken = await httpContext.GetTokenAsync("access_token");
            var _graphClient = graphClient.GetByAccessCode(incomingToken);

            Guid entraId = graphClient.GetUserEntraId(incomingToken!);

            Models.User? user = _context.Users.Where(user => user.Id == entraId).FirstOrDefault();

            if (user == null)
            {
                Logger.Log($"No user was found with id {entraId}, creating a new user");

                Graph.User? me = await _graphClient.Me.GetAsync();

                if (me == null || me.Id == null || me.UserPrincipalName == null)
                {
                    Logger.Log("Unable to obtain info about the user from graph", Logger.LogLevel.Warning);
                    return null;
                }

                user = new Models.User()
                {
                    Username = me.UserPrincipalName,
                    Id = Guid.Parse(me.Id)
                };

                _context.Users.Add(user);
                _context.SaveChanges();
            }

            return user;
        }

        /// <summary>
        /// Gets the stored user info that made the request, or null if the user doesnt exist
        /// </summary>
        /// <param name="httpContext">The http context of the request</param>
        /// <param name="_context">The database context</param>
        /// <param name="graphClient">The graph client to use</param>
        /// <returns>The user that made the request</returns>
        public static async Task<Models.User?> Get(HttpContext httpContext, LogbookDBContext _context, GraphClient graphClient)
        {
            var incomingToken = await httpContext.GetTokenAsync("access_token");

            Guid entraId = graphClient.GetUserEntraId(incomingToken!);

            Models.User? user = _context.Users.Where(user => user.Id == entraId).FirstOrDefault();

            return user;
        }

        /// <summary>
        /// Gets information about the caller that presented the access token
        /// </summary>
        /// <param name="accessToken">The token the user presented</param>
        /// <returns>A tokencaller object that contain information about the caller</returns>
        public static DTO.TokenCaller GetCallerByToken(string accessToken)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(accessToken);
            string? oid = jwt.Claims.FirstOrDefault(c => c.Type == "oid")?.Value;
            string? upn = jwt.Claims.FirstOrDefault(c => c.Type == "upn")?.Value;
            string? given_name = jwt.Claims.FirstOrDefault(c => c.Type == "given_name")?.Value;
            string? family_name = jwt.Claims.FirstOrDefault(c => c.Type == "family_name")?.Value;
            return new DTO.TokenCaller() {
                
            Id = Guid.Parse(oid!),
            UserPrincipalName = upn!,
            DisplayName = $"{given_name} {family_name}"
            };
        }
    }
}