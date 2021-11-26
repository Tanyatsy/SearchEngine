using AutocompleteService.Messages.Events.Interfaces.IDomainEvent;
using System.Collections.Generic;

namespace AutocompleteService.Messages.Events
{
    public class SearchesEvent : IDomainEvent {
        public SearchesEvent(string text, List<string> searches)
        {
            this.Searches = searches;
            this.Text = text;
        }

        public List<string> Searches { get; set; }
        public string Text { get; set; }
    }
}
