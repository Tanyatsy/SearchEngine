using SearchService.Models;
using System;
using System.Collections.Generic;
using Users.Domain.Events.Interfaces;

namespace SearchService.Messages.Events
{
    public class SearchDataEvent : IDomainEvent {
        public SearchDataEvent(
            List<PageData> pageData,
            string searchText)
        {
            this.PageData = pageData;
            this.SearchText = searchText;
        }

        public List<PageData> PageData;
        public string SearchText { get; set; }
    }
}
