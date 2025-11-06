using Logbook.Data;
using Graph = Microsoft.Graph.Models;
using Microsoft.AspNetCore.Authentication;
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
        /// <returns></returns>
        public static async Task<Models.User?> GetOrCreate(HttpContext httpContext, LogbookDBContext _context)
        {
            var incomingToken = await httpContext.GetTokenAsync("access_token");
            var graphClient = GraphClient.GetByAccessCode(incomingToken);

            Guid entraId = GraphClient.GetUserEntraId(incomingToken!);

            Models.User? user = _context.Users.Where(user => user.EntraId == entraId).FirstOrDefault();

            if (user == null)
            {
                Logger.Log($"No user was found with id {entraId}, creating a new user");

                Graph.User? me = await graphClient.Me.GetAsync();

                if (me == null || me.Id == null || me.UserPrincipalName == null)
                {
                    Logger.Log("Unable to obtain info about the user from graph", Logger.LogLevel.Warning);
                    return null;
                }

                user = new Models.User()
                {
                    UserName = me.UserPrincipalName,
                    EntraId = Guid.Parse(me.Id)
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
        /// <returns>The user that made the request</returns>
        public static async Task<Models.User?> Get(HttpContext httpContext, LogbookDBContext _context)
        {
            var incomingToken = await httpContext.GetTokenAsync("access_token");
            var graphClient = GraphClient.GetByAccessCode(incomingToken);

            Guid entraId = GraphClient.GetUserEntraId(incomingToken!);

            Models.User? user = _context.Users.Where(user => user.EntraId == entraId).FirstOrDefault();

            return user;
        }
    }
}