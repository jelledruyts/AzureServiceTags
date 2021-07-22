using System.Collections.Generic;
using System.Threading.Tasks;
using AzureServiceTags.WebApp.Models;
using AzureServiceTags.WebApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AzureServiceTags.WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IPLookupController : ControllerBase
    {
        private readonly ILogger<ServiceTagController> logger;
        private readonly IPLookupService ipLookupService;

        public IPLookupController(ILogger<ServiceTagController> logger, IPLookupService ipLookupService)
        {
            this.logger = logger;
            this.ipLookupService = ipLookupService;
        }

        [HttpPost("")]
        [HttpGet("")]
        public async Task<IList<IPLookupResult>> Get(string ipAddresses)
        {
            return await this.ipLookupService.Lookup(ipAddresses);
        }
    }
}