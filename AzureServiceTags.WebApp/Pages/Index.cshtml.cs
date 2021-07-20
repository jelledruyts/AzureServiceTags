using System;
using System.Collections.Generic;
using System.Linq;
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

        [BindProperty(SupportsGet = true)]
        public string IPAddresses { get; set; }

        public IList<ServiceTagListFile> ServiceTagListFiles { get; set; }
        public IList<IPAddressLookupResult> IPAddressLookupResults { get; set; }

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
            if (!string.IsNullOrWhiteSpace(this.IPAddresses))
            {
                this.IPAddressLookupResults = new List<IPAddressLookupResult>();
                foreach (var ipAddressString in this.IPAddresses.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(ip => ip.Trim()))
                {
                    var ipAddressLookupResult = new IPAddressLookupResult(ipAddressString);
                    this.IPAddressLookupResults.Add(ipAddressLookupResult);
                    if (System.Net.IPAddress.TryParse(ipAddressString, out System.Net.IPAddress ipAddress))
                    {
                        ipAddressLookupResult.IsIPAddressValid = true;
                        foreach (var serviceTagListFile in this.ServiceTagListFiles)
                        {
                            foreach (var serviceTag in serviceTagListFile.ServiceTagList.Values)
                            {
                                foreach (var addressPrefix in serviceTag.Properties.AddressPrefixes)
                                {
                                    var addressRange = IPAddressRange.Parse(addressPrefix);
                                    if (addressRange.Contains(ipAddress))
                                    {
                                        ipAddressLookupResult.MatchedServiceTags.Add(new MatchedServiceTag(ipAddressString, serviceTagListFile.ServiceTagList.Cloud, serviceTag, addressPrefix));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}