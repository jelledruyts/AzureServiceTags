using System.Collections.Generic;

namespace AzureServiceTags.WebApp.Models
{
    public class IPAddressLookupResult
    {
        public string IPAddress { get; set; }
        public bool IsIPAddressValid { get; set; }
        public IList<MatchedServiceTag> MatchedServiceTags { get; set; }

        public IPAddressLookupResult(string ipAddress)
        {
            this.IPAddress = ipAddress;
            this.MatchedServiceTags = new List<MatchedServiceTag>();
        }
    }
}