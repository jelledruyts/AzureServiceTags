namespace AzureServiceTags.WebApp.Models
{
    public class ServiceTagProperties
    {
        public string[] AddressPrefixes { get; set; }
        public int ChangeNumber { get; set; }
        public string[] NetworkFeatures { get; set; } // "API", "NSG", "UDR", "FW", "VSE"
        public string Region { get; set; }
        public int RegionId { get; set; }
        public string Platform { get; set; } // Value is always "Azure"
        public string SystemService { get; set; }
    }
}