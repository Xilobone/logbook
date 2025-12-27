using System.IdentityModel.Tokens.Jwt;
using Logbook.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace Logbook.Util
{
    /// <summary>
    /// Class for authentication related util functions
    /// </summary>
    public class Auth
    {
        /// <summary>
        /// Gets the caller of the endpoint based on the access token provided
        /// in the http context
        /// </summary>
        /// <param name="context">The http context to use</param>
        /// <returns>An object containing info about the caller, or null if no valid token was provided</returns>
        public static async Task<DTO.TokenCaller?> GetCallerByHttpContext(HttpContext context)
        {
            string? accessToken = await context.GetTokenAsync("access_token");

            if (string.IsNullOrEmpty(accessToken))
            {
                Logger.Log("Endpoint was reached without a valid access token", Logger.LogLevel.Error);
                return null;
            }

            //get required fields from token
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(accessToken);
            string? oid = jwt.Claims.FirstOrDefault(c => c.Type == "oid")?.Value;
            string? upn = jwt.Claims.FirstOrDefault(c => c.Type == "upn")?.Value;
            string? given_name = jwt.Claims.FirstOrDefault(c => c.Type == "given_name")?.Value;
            string? family_name = jwt.Claims.FirstOrDefault(c => c.Type == "family_name")?.Value;

            //return token caller object
            return new DTO.TokenCaller()
            {

                Id = Guid.Parse(oid!),
                UserPrincipalName = upn!,
                DisplayName = $"{given_name} {family_name}"
            };
        }


        /// <summary>
        /// Validates an api request based on the users access token
        /// </summary>
        /// <param name="controller">The api controller that the request was for</param>
        /// <param name="dbContext">The database context to use</param>
        /// <returns>A tuple containing a bool indicating if the request was valid, a user if the request was valid,
        /// and an ActionResult displaying the error message if the request was not valid</returns>
        public static async Task<(bool isValid, Models.User userId, IActionResult error)> ValidateRequest(ControllerBase controller, LogbookDBContext dbContext)
        {
            DTO.TokenCaller? caller = await GetCallerByHttpContext(controller.HttpContext);

            //we love a good null!
            if (caller == null) return (false, null!, controller.Unauthorized("No valid token was provided"));

            Models.User? user = User.GetUserByCaller(caller, dbContext);
            if (user == null) return (false, null!, controller.Forbid("User is not registered"));

            return (true, user, controller.Ok());

        }

    }
}