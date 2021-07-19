using System.Threading.Tasks;
using AzureServiceTags.WebApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AzureServiceTags.WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceTagController : ControllerBase
    {
        private readonly ILogger<ServiceTagController> logger;
        private readonly ServiceTagProvider serviceTagProvider;

        public ServiceTagController(ILogger<ServiceTagController> logger, ServiceTagProvider serviceTagProvider)
        {
            this.logger = logger;
            this.serviceTagProvider = serviceTagProvider;
        }

        [HttpPost(nameof(ClearCache))]
        public async Task<IActionResult> ClearCache()
        {
            await this.serviceTagProvider.ClearCacheAsync();
            return Ok();
        }
    }
}