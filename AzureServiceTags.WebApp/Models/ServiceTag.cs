namespace AzureServiceTags.WebApp.Models
{
    public class ServiceTag
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public ServiceTagProperties Properties { get; set; }
    }
}