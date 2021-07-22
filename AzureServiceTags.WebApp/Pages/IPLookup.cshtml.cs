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
    public class IPLookupModel : PageModel
    {
        private readonly ILogger<IndexModel> logger;
        private readonly ServiceTagProvider serviceTagProvider;

        [BindProperty(SupportsGet = true)]
        public string IPAddresses { get; set; }

        public IList<IPAddressLookupResult> IPAddressLookupResults { get; set; }

        public IPLookupModel(ILogger<IndexModel> logger, ServiceTagProvider serviceTagProvider)
        {
            this.logger = logger;
            this.serviceTagProvider = serviceTagProvider;
        }

        public void OnGet()
        {
        }

        public async Task OnPost()
        {
            var serviceTagListFiles = await this.serviceTagProvider.GetAllServiceTagListFilesAsync();
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
                        foreach (var serviceTagListFile in serviceTagListFiles)
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