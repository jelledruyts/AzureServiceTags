using System.Collections.Generic;

namespace AzureServiceTags.WebApp.Models
{
    public class IPLookupResult
    {
        public string IPAddress { get; set; }
        public bool IsIPAddressValid { get; set; }
        public IList<MatchedServiceTag> MatchedServiceTags { get; set; }

        public IPLookupResult(string ipAddress)
        {
            this.IPAddress = ipAddress;
            this.MatchedServiceTags = new List<MatchedServiceTag>();
        }
    }
}