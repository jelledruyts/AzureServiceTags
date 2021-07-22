namespace AzureServiceTags.WebApp.Models
{
    public class ServiceTag
    {
        public string Id { get; set; }
        public string Name { get; set; } // Value is always the same as the Id
        public ServiceTagProperties Properties { get; set; }
    }
}