using System.Collections.Generic;

namespace AzureServiceTags.WebApp.Models
{
    public class ServiceTagList
    {
        public int ChangeNumber { get; set; }
        public string Cloud { get; set; }
        public ICollection<ServiceTag> Values { get; set; }
    }
}