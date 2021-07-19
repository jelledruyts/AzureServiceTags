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
            if ((DateTimeOffset.UtcNow - this.Metadata.LastDownloadAttemptedTime).TotalHours <= 1)
            {
                // Don't attempt to refresh if the last attempt was less than an hour ago.
                return false;
            }
            else if (this.Metadata.LastDownloadSucceededTime.HasValue && (DateTimeOffset.UtcNow - this.Metadata.LastDownloadSucceededTime.Value).TotalHours <= 24)
            {
                // Don't attempt to refresh if the last succeeded attempt was less than a day ago (service tags are published weekly).
                return false;
            }
            return true;
        }
    }
}