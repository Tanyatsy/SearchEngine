using System;

namespace SearchService.Models
{
    public class PageData
    {
        public Guid? Id { get; set; }
        public string Link { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
    }
}
