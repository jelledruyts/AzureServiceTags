using System.Threading.Tasks;
using AzureServiceTags.WebApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AzureServiceTags.WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CacheController : ControllerBase
    {
        private readonly ILogger<ServiceTagController> logger;
        private readonly ServiceTagProvider serviceTagProvider;

        public CacheController(ILogger<ServiceTagController> logger, ServiceTagProvider serviceTagProvider)
        {
            this.logger = logger;
            this.serviceTagProvider = serviceTagProvider;
        }

        [Route(nameof(Clear))]
        [HttpPost]
        [HttpGet]
        public async Task<IActionResult> Clear()
        {
            this.logger.LogInformation("Received request to clear cache");
            await this.serviceTagProvider.ClearCacheAsync();
            return Ok();
        }
    }
}