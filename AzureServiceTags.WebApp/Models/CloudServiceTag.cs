namespace AzureServiceTags.WebApp.Models
{
    public class CloudServiceTag
    {
        public string CloudId { get; set; }
        public int CloudChangeNumber { get; set; }
        public ServiceTag ServiceTag { get; set; }

        public CloudServiceTag(ServiceTagList serviceTagList, ServiceTag serviceTag)
        {
            this.CloudId = serviceTagList.Cloud;
            this.CloudChangeNumber = serviceTagList.ChangeNumber;
            this.ServiceTag = serviceTag;
        }
    }
}