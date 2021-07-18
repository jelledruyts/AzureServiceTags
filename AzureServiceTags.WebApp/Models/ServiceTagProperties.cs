namespace AzureServiceTags.WebApp.Models
{
    public class ServiceTagProperties
    {
        public string[] AddressPrefixes { get; set; }
        public object ChangeNumber { get; set; } // In download: int; in API: string
        public string[] NetworkFeatures { get; set; }
        public string Region { get; set; }
        public int RegionId { get; set; } // Only in download
        public string State { get; set; } // Only in API
        public string Platform { get; set; } // Only in download
        public string SystemService { get; set; }
    }
}