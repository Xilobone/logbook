using Logbook.Data;
using Graph = Microsoft.Graph.Models;
using Microsoft.AspNetCore.Authentication;
using System.IdentityModel.Tokens.Jwt;
namespace Logbook.Util
{
    public class User
    {
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
    }
}