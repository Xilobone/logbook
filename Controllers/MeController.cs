using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Microsoft.Identity.Client;

namespace Logbook.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MeController : ControllerBase
    {
        private readonly IConfidentialClientApplication _confidentialClient;

        public MeController(IConfiguration config)
        {
            _confidentialClient = ConfidentialClientApplicationBuilder.Create(config["AzureAd:ClientId"])
                .WithClientSecret(config["AzureAd:ClientSecret"])
                .WithAuthority($"https://login.microsoftonline.com/{config["AzureAd:TenantId"]}")
                .Build();
        }

        [HttpGet]
        public async Task<IActionResult> GetMe()
        {
            var incomingToken = await HttpContext.GetTokenAsync("access_token");

            var credential = new MsalOnBehalfOfCredential(_confidentialClient, incomingToken!);

            var graphClient = new GraphServiceClient(credential);

            var me = await graphClient.Me.GetAsync();

            return Ok(new { me!.DisplayName, me.UserPrincipalName });
        }
    }
}