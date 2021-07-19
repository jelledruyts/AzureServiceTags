using System;
using System.IO;

namespace AzureServiceTags.WebApp.Models
{
    public class ServiceTagListFileMetadata
    {
        public string DownloadUrl { get; set; }
        public string FileName { get; set; }
        public DateTimeOffset? LastDownloadSucceededTime { get; set; }
        public DateTimeOffset LastDownloadAttemptedTime { get; set; }

        public ServiceTagListFileMetadata()
        {
        }

        public ServiceTagListFileMetadata(string downloadUrl)
        {
            this.DownloadUrl = downloadUrl;
            this.FileName = Path.GetFileName(this.DownloadUrl);
            this.LastDownloadAttemptedTime = DateTimeOffset.UtcNow;
            this.LastDownloadSucceededTime = this.LastDownloadAttemptedTime;
        }
    }
}