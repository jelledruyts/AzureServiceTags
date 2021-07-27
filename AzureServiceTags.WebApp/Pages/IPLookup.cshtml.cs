using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureServiceTags.WebApp.Models;
using AzureServiceTags.WebApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace AzureServiceTags.WebApp.Pages
{
    public class IPLookupModel : PageModel
    {
        private readonly ILogger<IndexModel> logger;
        private readonly IPLookupService ipLookupService;

        [BindProperty(SupportsGet = true)]
        public string IPAddresses { get; set; }

        public IList<IPLookupResult> IPLookupResults { get; set; }

        public IPLookupModel(ILogger<IndexModel> logger, IPLookupService ipLookupService)
        {
            this.logger = logger;
            this.ipLookupService = ipLookupService;
        }

        public Task OnGet()
        {
            return ExecuteCoreAsync();
        }

        public Task OnPost()
        {
            return ExecuteCoreAsync();
        }

        private async Task ExecuteCoreAsync()
        {
            this.IPLookupResults = await this.ipLookupService.Lookup(this.IPAddresses);
        }
    }
}