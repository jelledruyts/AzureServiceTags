using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using AzureServiceTags.WebApp.Models;
using AzureServiceTags.WebApp.Services;
using NetTools;

namespace AzureServiceTags.WebApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> logger;
        private readonly ServiceTagProvider serviceTagProvider;

        [BindProperty]
        public string IPAddress { get; set; }

        public IList<ServiceTagListFile> ServiceTagListFiles { get; set; }
        public string WarningMessage { get; set; }
        public IList<MatchedServiceTag> MatchedServiceTags { get; set; }

        public IndexModel(ILogger<IndexModel> logger, ServiceTagProvider serviceTagProvider)
        {
            this.logger = logger;
            this.serviceTagProvider = serviceTagProvider;
        }

        public async Task OnGet()
        {
            this.ServiceTagListFiles = await this.serviceTagProvider.GetAllServiceTagListFilesAsync();
        }

        public async Task OnPost()
        {
            this.ServiceTagListFiles = await this.serviceTagProvider.GetAllServiceTagListFilesAsync();
            if (!System.Net.IPAddress.TryParse(this.IPAddress, out System.Net.IPAddress ipAddress))
            {
                this.WarningMessage = $"{this.IPAddress} is not a valid IP address.";
            }
            else
            {
                this.MatchedServiceTags = new List<MatchedServiceTag>();
                foreach (var serviceTagListFile in this.ServiceTagListFiles)
                {
                    foreach (var serviceTag in serviceTagListFile.ServiceTagList.Values)
                    {
                        foreach (var addressPrefix in serviceTag.Properties.AddressPrefixes)
                        {
                            var addressRange = IPAddressRange.Parse(addressPrefix);
                            if (addressRange.Contains(ipAddress))
                            {
                                this.MatchedServiceTags.Add(new MatchedServiceTag(this.IPAddress, serviceTagListFile.ServiceTagList.Cloud, serviceTag, addressPrefix));
                            }
                        }
                    }
                }
            }
        }
    }
}