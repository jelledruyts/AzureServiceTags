using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AzureServiceTags.WebApp.Models;

namespace AzureServiceTags.WebApp.Services
{
    public class ServiceTagProvider
    {
        private static JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions { WriteIndented = true, PropertyNameCaseInsensitive = true };
        private readonly ILogger<ServiceTagProvider> logger;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly string basePath;
        private readonly Dictionary<string, ServiceTagListFile> serviceTagListFiles = new Dictionary<string, ServiceTagListFile>();

        public ServiceTagProvider(ILogger<ServiceTagProvider> logger, IHttpClientFactory httpClientFactory)
        {
            this.logger = logger;
            this.httpClientFactory = httpClientFactory;
            this.basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DownloadCache");
            // Ensure that the directory exists.
            Directory.CreateDirectory(this.basePath);
        }

        public async Task<IList<ServiceTagListFile>> GetAllServiceTagListFilesAsync()
        {
            var tasks = Constants.CloudIds.AllCloudIds.Select(id => GetServiceTagListFileAsync(id));
            await Task.WhenAll(tasks);
            return tasks.Select(t => t.Result).ToArray();
        }

        public async Task<ServiceTagListFile> GetServiceTagListFileAsync(string cloudId)
        {
            if (!this.serviceTagListFiles.ContainsKey(cloudId) || this.serviceTagListFiles[cloudId].NeedsRefresh())
            {
                this.logger.LogDebug($"Loading service tag list for cloud \"{cloudId}\"");
                var serviceTagListFile = await LoadServiceTagListFileAsync(cloudId, false);
                if (serviceTagListFile.NeedsRefresh())
                {
                    this.logger.LogDebug($"Refreshing service tag list for cloud \"{cloudId}\"");
                    serviceTagListFile = await LoadServiceTagListFileAsync(cloudId, true);
                }
                this.serviceTagListFiles[cloudId] = serviceTagListFile;
            }
            return this.serviceTagListFiles[cloudId];
        }

        private async Task<ServiceTagListFile> LoadServiceTagListFileAsync(string cloudId, bool forceRefresh)
        {
            var fileName = GetServiceTagListFileName(cloudId);
            var metadataFileName = GetServiceTagListMetadataFileName(cloudId);
            if (forceRefresh || !File.Exists(fileName))
            {
                this.logger.LogDebug($"Downloading service tag list for cloud \"{cloudId}\" to file \"{fileName}\"");
                await DownloadServiceTagListFileAsync(cloudId, fileName, metadataFileName);
            }
            this.logger.LogDebug($"Reading service tag list for cloud \"{cloudId}\" from file \"{fileName}\"");
            using (var fileStream = File.OpenRead(fileName))
            {
                var serviceTagList = await JsonSerializer.DeserializeAsync<ServiceTagList>(fileStream, jsonSerializerOptions);

                this.logger.LogDebug($"Reading service tag list metadata for cloud \"{cloudId}\" from file \"{metadataFileName}\"");
                using (var metadataFileStream = File.OpenRead(metadataFileName))
                {
                    var serviceTagListMetadata = await JsonSerializer.DeserializeAsync<ServiceTagListFileMetadata>(metadataFileStream, jsonSerializerOptions);
                    return new ServiceTagListFile(serviceTagListMetadata, serviceTagList);
                }
            }
        }

        private async Task DownloadServiceTagListFileAsync(string cloudId, string fileName, string metadataFileName)
        {
            var downloadId = GetDownloadId(cloudId);
            var confirmationUrl = $"https://www.microsoft.com/en-us/download/confirmation.aspx?id={downloadId}";
            this.logger.LogDebug($"Downloading service tag confirmation page for cloud \"{cloudId}\"");
            var httpClient = this.httpClientFactory.CreateClient();
            var confirmationPage = await httpClient.GetStringAsync(confirmationUrl);
            var downloadUrlStartTag = "meta http-equiv=\"refresh\" content=\"0;url=";
            var downloadUrlStartIndex = confirmationPage.IndexOf(downloadUrlStartTag, StringComparison.InvariantCultureIgnoreCase);
            if (downloadUrlStartIndex < 0)
            {
                this.logger.LogError($"Could not find start of download URL in confirmation page for cloud \"{cloudId}\"");
                throw new ArgumentException($"Could not determine download URL for cloud \"{cloudId}\".");
            }
            else
            {
                downloadUrlStartIndex += downloadUrlStartTag.Length;
                var downloadUrlEndIndex = confirmationPage.IndexOf('"', downloadUrlStartIndex);
                if (downloadUrlEndIndex < 0)
                {
                    this.logger.LogError($"Could not find end of download URL in confirmation page for cloud \"{cloudId}\"");
                    throw new ArgumentException($"Could not determine download URL for cloud \"{cloudId}\".");
                }
                else
                {
                    var downloadUrl = confirmationPage.Substring(downloadUrlStartIndex, downloadUrlEndIndex - downloadUrlStartIndex);
                    this.logger.LogDebug($"Downloading service tag list for cloud \"{cloudId}\" from \"{downloadUrl}\"");
                    using (var response = await httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead))
                    using (var responseStream = await response.Content.ReadAsStreamAsync())
                    {
                        this.logger.LogDebug($"Saving service tag list for cloud \"{cloudId}\" to \"{fileName}\"");
                        using (var outputStream = File.Open(fileName, FileMode.Create))
                        {
                            await responseStream.CopyToAsync(outputStream);
                        }
                        this.logger.LogDebug($"Saving service tag list metadata for cloud \"{cloudId}\" to \"{metadataFileName}\"");
                        var metadata = new ServiceTagListFileMetadata(downloadUrl);
                        using (var metadataOutputStream = File.Open(metadataFileName, FileMode.Create))
                        {
                            await JsonSerializer.SerializeAsync<ServiceTagListFileMetadata>(metadataOutputStream, metadata, jsonSerializerOptions);
                        }
                    }
                }
            }
        }

        private string GetServiceTagListFileName(string cloudId)
        {
            return Path.Combine(this.basePath, $"ServiceTags-{cloudId}.json");
        }

        private string GetServiceTagListMetadataFileName(string cloudId)
        {
            return Path.Combine(this.basePath, $"ServiceTags-{cloudId}.metadata.json");
        }

        private static string GetDownloadId(string cloudId)
        {
            if (string.Equals(cloudId, Constants.CloudIds.Public, StringComparison.InvariantCultureIgnoreCase))
            {
                // Public: https://www.microsoft.com/en-us/download/details.aspx?id=56519
                return "56519";
            }
            else if (string.Equals(cloudId, Constants.CloudIds.China, StringComparison.InvariantCultureIgnoreCase))
            {
                // China: http://www.microsoft.com/en-us/download/details.aspx?id=57062
                return "57062";
            }
            else if (string.Equals(cloudId, Constants.CloudIds.AzureGovernment, StringComparison.InvariantCultureIgnoreCase))
            {
                // US Gov: http://www.microsoft.com/en-us/download/details.aspx?id=57063
                return "57063";
            }
            else if (string.Equals(cloudId, Constants.CloudIds.AzureGermany, StringComparison.InvariantCultureIgnoreCase))
            {
                // Germany: http://www.microsoft.com/en-us/download/details.aspx?id=57064
                return "57064";
            }
            else
            {
                throw new ArgumentException($"Cloud identifier \"{cloudId}\" doesn't have a downloadable service tag list.");
            }
        }
    }
}