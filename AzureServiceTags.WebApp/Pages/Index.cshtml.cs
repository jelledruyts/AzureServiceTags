using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using AzureServiceTags.WebApp.Models;
using AzureServiceTags.WebApp.Services;

namespace AzureServiceTags.WebApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> logger;
        private readonly ServiceTagProvider serviceTagProvider;

        public IList<ServiceTagListFile> ServiceTagListFiles { get; set; }

        public IndexModel(ILogger<IndexModel> logger, ServiceTagProvider serviceTagProvider)
        {
            this.logger = logger;
            this.serviceTagProvider = serviceTagProvider;
        }

        public async Task OnGet()
        {
            this.ServiceTagListFiles = await this.serviceTagProvider.GetAllServiceTagListFilesAsync();
        }

        public void OnPost()
        {
        }
    }
}