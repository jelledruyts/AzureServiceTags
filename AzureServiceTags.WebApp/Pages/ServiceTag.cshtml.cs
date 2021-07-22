using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureServiceTags.WebApp.Services;
using AzureServiceTags.WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace AzureServiceTags.WebApp.Pages
{
    public class ServiceTagModel : PageModel
    {
        private readonly ILogger<IndexModel> logger;
        private readonly ServiceTagProvider serviceTagProvider;

        [BindProperty(SupportsGet = true)]
        public string ServiceTagId { get; set; }

        public IList<string> TopLevelServiceTags { get; set; }
        public IList<string> ServiceTagIds { get; set; }
        public IList<CloudServiceTag> ServiceTags { get; set; }

        public ServiceTagModel(ILogger<IndexModel> logger, ServiceTagProvider serviceTagProvider)
        {
            this.logger = logger;
            this.serviceTagProvider = serviceTagProvider;
        }

        public async Task OnGet()
        {
            var serviceTagListFiles = await this.serviceTagProvider.GetAllServiceTagListFilesAsync();
            if (string.IsNullOrWhiteSpace(this.ServiceTagId))
            {
                // No specific Service Tag (prefix) was requested, return top-level Service Tags (i.e. anything before the first '.').
                this.TopLevelServiceTags = serviceTagListFiles.SelectMany(f => f.ServiceTagList.Values)
                    .Select(l => l.Id.Contains('.') ? l.Id.Substring(0, l.Id.IndexOf('.')) : l.Id)
                    .Distinct().OrderBy(n => n).ToArray();
            }
            else if (this.ServiceTagId.EndsWith('*'))
            {
                // Find Service Tag ID's starting with the requested ID.
                this.ServiceTagIds = new List<string>();
                var matchString = this.ServiceTagId.TrimEnd('*');
                this.ServiceTagIds = serviceTagListFiles.SelectMany(f => f.ServiceTagList.Values)
                    .Where(s => string.Equals(matchString, s.Id, StringComparison.OrdinalIgnoreCase) || s.Id.StartsWith(matchString + '.', StringComparison.OrdinalIgnoreCase))
                    .Select(s => s.Id).Distinct().OrderBy(s => s).ToArray();
            }
            else
            {
                // Return the requested Service Tag.
                this.ServiceTags = new List<CloudServiceTag>();
                foreach (var serviceTagList in serviceTagListFiles.Select(f => f.ServiceTagList))
                {
                    foreach (var serviceTag in serviceTagList.Values.Where(s => string.Equals(this.ServiceTagId, s.Id, StringComparison.OrdinalIgnoreCase)))
                    {
                        this.ServiceTags.Add(new CloudServiceTag(serviceTagList, serviceTag));
                    }
                }
            }
        }

        public void OnPost()
        {
        }
    }
}