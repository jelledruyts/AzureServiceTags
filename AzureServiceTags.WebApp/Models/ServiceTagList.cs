namespace AzureServiceTags.WebApp.Models
{
    public class ServiceTagList
    {
        public string Name { get; set; } // Only in API
        public string Id { get; set; } // Only in API
        public string Type { get; set; } // Only in API
        public object ChangeNumber { get; set; } // In download: int; in API: string
        public string Cloud { get; set; }
        public ServiceTag[] Values { get; set; }
    }
}