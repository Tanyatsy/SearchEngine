using SearchService.Messages;
using SearchService.Messages.Events;
using SearchService.Models;
using System;
using System.Collections.Generic;

namespace SearchService.Messges.Commands
{
    public class SearchDataCommand : AggregateRoot
    {
        private string searchText;
        public List<PageData> PageData { get; set; }
        public string SearchText
        {
            get
            {
                return searchText;
            }
            set
            {
                searchText = value;
                AddEvent(new SearchDataEvent(PageData, searchText));
            }
        }
    }
}
