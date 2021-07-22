using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AzureServiceTags.WebApp.Models;
using Microsoft.Extensions.Logging;
using NetTools;

namespace AzureServiceTags.WebApp.Services
{
    public class IPLookupService
    {
        private readonly ILogger<IPLookupService> logger;
        private readonly ServiceTagProvider serviceTagProvider;

        public IPLookupService(ILogger<IPLookupService> logger, ServiceTagProvider serviceTagProvider)
        {
            this.logger = logger;
            this.serviceTagProvider = serviceTagProvider;
        }

        public async Task<IList<IPLookupResult>> Lookup(string ipAddresses)
        {
            var ipLookupResults = new List<IPLookupResult>();
            if (!string.IsNullOrWhiteSpace(ipAddresses))
            {
                var serviceTagListFiles = await this.serviceTagProvider.GetAllServiceTagListFilesAsync();
                foreach (var ipAddressString in ipAddresses.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(ip => ip.Trim()))
                {
                    var ipLookupResult = new IPLookupResult(ipAddressString);
                    ipLookupResults.Add(ipLookupResult);
                    if (IPAddress.TryParse(ipAddressString, out IPAddress ipAddress))
                    {
                        ipLookupResult.IsIPAddressValid = true;
                        foreach (var serviceTagListFile in serviceTagListFiles)
                        {
                            foreach (var serviceTag in serviceTagListFile.ServiceTagList.Values)
                            {
                                foreach (var addressPrefix in serviceTag.Properties.AddressPrefixes)
                                {
                                    var addressRange = IPAddressRange.Parse(addressPrefix);
                                    if (addressRange.Contains(ipAddress))
                                    {
                                        ipLookupResult.MatchedServiceTags.Add(new MatchedServiceTag(ipAddressString, serviceTagListFile.ServiceTagList.Cloud, serviceTag, addressPrefix));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return ipLookupResults;
        }
    }
}