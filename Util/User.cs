using Logbook.Data;
using MSGraph = Microsoft.Graph.Models;
using Microsoft.AspNetCore.Authentication;

namespace Logbook.Util
{
    /// <summary>
    /// Class containing util functions regarding users
    /// </summary>
    public class User
    {
        /// <summary>
        /// Gets the user from the database that called the endpoint
        /// </summary>
        /// <param name="caller">Data about the endpoint caller</param>
        /// <param name="_context">The database context to look in</param>
        /// <param name="logLevelIfNotFound">The loglevel to log a message if the user was not found, defaults to error</param>
        /// <returns>The user that called the endpoint, or null if this user doesn't exist</returns>
        public static Models.User? GetUserByCaller(DTO.TokenCaller caller, LogbookDBContext _context, Logger.LogLevel logLevelIfNotFound = Logger.LogLevel.Error)
        {
            Models.User? user = _context.Users.Where(u => u.Id == caller.Id).FirstOrDefault();

            if (user == null) Logger.Log($"Endpoint was called by user with id {caller.Id}, but this user was not found in the db", logLevelIfNotFound);

            return user;
        }
    }
}