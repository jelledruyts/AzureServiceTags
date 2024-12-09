using System;
using System.Collections.Generic;
using System.IO;
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
            var serviceTagListFiles = new List<ServiceTagListFile>(Constants.CloudIds.AllCloudIds.Length);
            foreach (var cloudId in Constants.CloudIds.AllCloudIds)
            {
                var serviceTagListFile = await GetServiceTagListFileAsync(cloudId);
                if (serviceTagListFile.ServiceTagList != null)
                {
                    serviceTagListFiles.Add(serviceTagListFile);
                }
            }
            return serviceTagListFiles;
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

        public Task ClearCacheAsync()
        {
            this.serviceTagListFiles.Clear();
            Directory.Delete(this.basePath, true);
            Directory.CreateDirectory(this.basePath);
            return Task.CompletedTask;
        }

        #region Helper Methods

        private async Task<ServiceTagListFile> LoadServiceTagListFileAsync(string cloudId, bool forceRefresh)
        {
            var fileName = GetServiceTagListFileName(cloudId);
            var metadataFileName = GetServiceTagListMetadataFileName(cloudId);

            if (forceRefresh || !File.Exists(fileName))
            {
                this.logger.LogInformation($"Downloading service tag list for cloud \"{cloudId}\" to file \"{fileName}\"");
                await DownloadServiceTagListFileAsync(cloudId, fileName, metadataFileName);
            }

            this.logger.LogDebug($"Reading service tag list for cloud \"{cloudId}\" from file \"{fileName}\"");
            var serviceTagList = await ReadFileAsync<ServiceTagList>(fileName);

            this.logger.LogDebug($"Reading service tag list metadata for cloud \"{cloudId}\" from file \"{metadataFileName}\"");
            var serviceTagListMetadata = await ReadFileAsync<ServiceTagListFileMetadata>(metadataFileName);

            return new ServiceTagListFile(serviceTagListMetadata, serviceTagList);
        }

        private async Task DownloadServiceTagListFileAsync(string cloudId, string fileName, string metadataFileName)
        {
            ServiceTagListFileMetadata metadata;
            try
            {
                var downloadId = GetDownloadId(cloudId);
                var detailsUrl = $"https://www.microsoft.com/en-us/download/details.aspx?id={downloadId}";
                this.logger.LogDebug($"Downloading service tag confirmation page for cloud \"{cloudId}\"");
                var httpClient = this.httpClientFactory.CreateClient();
                var detailsPage = await httpClient.GetStringAsync(detailsUrl);
                var downloadUrlStartIndex = detailsPage.IndexOf("https://download.microsoft.com", StringComparison.OrdinalIgnoreCase);
                var downloadUrlEndIndex = detailsPage.IndexOf("\"", downloadUrlStartIndex, StringComparison.OrdinalIgnoreCase);
                var downloadUrl = detailsPage.Substring(downloadUrlStartIndex, downloadUrlEndIndex - downloadUrlStartIndex);
                this.logger.LogDebug($"Downloading service tag list for cloud \"{cloudId}\" from \"{downloadUrl}\"");
                using (var response = await httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead))
                using (var responseStream = await response.Content.ReadAsStreamAsync())
                {
                    this.logger.LogDebug($"Saving service tag list for cloud \"{cloudId}\" to \"{fileName}\"");
                    using (var outputStream = File.Open(fileName, FileMode.Create))
                    {
                        await responseStream.CopyToAsync(outputStream);
                    }
                    metadata = new ServiceTagListFileMetadata(downloadUrl);
                }
            }
            catch (Exception exc)
            {
                this.logger.LogError(exc, $"Could not download service tag list for cloud \"{cloudId}\"");

                this.logger.LogDebug($"Reading service tag list metadata for cloud \"{cloudId}\" from file \"{metadataFileName}\"");
                metadata = await ReadFileAsync<ServiceTagListFileMetadata>(metadataFileName);
                if (metadata == null)
                {
                    metadata = new ServiceTagListFileMetadata();
                }
                metadata.LastDownloadAttemptedTime = DateTimeOffset.UtcNow;
            }
            this.logger.LogDebug($"Saving service tag list metadata for cloud \"{cloudId}\" to \"{metadataFileName}\"");
            await WriteFileAsync<ServiceTagListFileMetadata>(metadataFileName, metadata);
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

        private async Task<T> ReadFileAsync<T>(string fileName) where T : class
        {
            if (!File.Exists(fileName))
            {
                this.logger.LogWarning($"Cannot read {typeof(T).Name} because file \"{fileName}\" does not exist.");
                return null;
            }
            using (var fileStream = File.OpenRead(fileName))
            {
                return await JsonSerializer.DeserializeAsync<T>(fileStream, jsonSerializerOptions);
            }
        }

        private async Task WriteFileAsync<T>(string fileName, T value)
        {
            using (var outputStream = File.Open(fileName, FileMode.Create))
            {
                await JsonSerializer.SerializeAsync<T>(outputStream, value, jsonSerializerOptions);
            }
        }

        #endregion
    }
}