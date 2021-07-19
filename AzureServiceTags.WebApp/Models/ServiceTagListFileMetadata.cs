using System;
using System.IO;

namespace AzureServiceTags.WebApp.Models
{
    public class ServiceTagListFileMetadata
    {
        public string DownloadUrl { get; set; }
        public string FileName { get; set; }
        public string Version { get; set; }
        public DateTimeOffset ValidFromDate { get; set; }

        public ServiceTagListFileMetadata()
        {
        }

        public ServiceTagListFileMetadata(string downloadUrl)
        {
            this.DownloadUrl = downloadUrl; // "https://download.microsoft.com/download/7/1/D/71D86715-5596-4529-9B13-DA13A5DE5B63/ServiceTags_Public_20210712.json"
            this.FileName = Path.GetFileName(this.DownloadUrl); // "ServiceTags_Public_20210712.json"
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(this.FileName); // "ServiceTags_Public_20210712"
            var index = fileNameWithoutExtension.LastIndexOf('_');
            if (index < 0)
            {
                throw new ArgumentException("Download URL isn't in expected format containing date pattern.");
            }
            var datePattern = fileNameWithoutExtension.Substring(index + 1); // "20210712"
            if (DateTimeOffset.TryParseExact(datePattern, "yyyyMMdd", null, System.Globalization.DateTimeStyles.AssumeUniversal, out DateTimeOffset dateValue))
            {
                this.Version = datePattern;
                this.ValidFromDate = dateValue;
            }
            else
            {
                throw new ArgumentException("Download URL isn't in expected format containing date pattern.");
            }
        }
    }
}