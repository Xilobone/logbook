using Logbook.Data;
using Logbook.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;

namespace Logbook.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RefreshController : ControllerBase
    {
        readonly LogbookDBContext _context;
        readonly IConfiguration _config;
        public RefreshController(LogbookDBContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpGet]
        public async Task<IActionResult> Test()
        {
            string result = "";
            foreach (User user in _context.Users)
            {
                string userUniqueId = user.EntraId.ToString() + "." + _config["AzureAd:TenantId"];
                GraphServiceClient graphClient = await GraphClient.GetGraphClientForUserAsync(_context, userUniqueId);
                Logger.Log("finished creating client");
                Microsoft.Graph.Models.User? u = await graphClient.Me.GetAsync();

                result += u.UserPrincipalName + ", ";


            }

            return Ok(result);
        }
    }
}