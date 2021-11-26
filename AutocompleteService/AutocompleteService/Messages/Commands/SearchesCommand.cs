using AutocompleteService.Messages;
using AutocompleteService.Messages.Events;
using System.Collections.Generic;

namespace AutocompleteService.Messges.Commands
{
    public class SearchesCommand : AggregateRoot
    {
        public List<string> Searches { get; set; }
        private string text;
        public string Text
        {
            get 
            {
                return text;
            }
            set
            {
                text = value;
                AddEvent(new SearchesEvent(value, Searches));
            }
        }
    }
}
