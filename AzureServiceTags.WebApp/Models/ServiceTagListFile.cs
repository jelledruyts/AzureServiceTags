using System;

namespace AzureServiceTags.WebApp.Models
{
    public class ServiceTagListFile
    {
        public ServiceTagListFileMetadata Metadata { get; set; }
        public ServiceTagList ServiceTagList { get; set; }

        public ServiceTagListFile(ServiceTagListFileMetadata metadata, ServiceTagList serviceTagList)
        {
            this.Metadata = metadata;
            this.ServiceTagList = serviceTagList;
        }

        public bool NeedsRefresh()
        {
            // The data is refreshed weekly, give it one extra day buffer before downloading again.
            return (DateTimeOffset.Now.Date - this.Metadata.ValidFromDate.Date).Days >= 8;
        }
    }
}