using System.Collections.Generic;

namespace CacheService.Models
{
    public class SearchData
    {
        public List<PageData> PageData;
        public string SearchText { get; set; }
    }
}
