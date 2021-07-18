namespace AzureServiceTags.WebApp.Models
{
    public class MatchedServiceTag
    {
        public string IPAddress { get; set; }
        public string CloudId { get; set; }
        public ServiceTag ServiceTag { get; set; }
        public string AddressPrefix { get; set; }

        public MatchedServiceTag(string ipAddress, string cloudId, ServiceTag serviceTag, string addressPrefix)
        {
            this.IPAddress = ipAddress;
            this.CloudId = cloudId;
            this.ServiceTag = serviceTag;
            this.AddressPrefix = addressPrefix;
        }
    }
}