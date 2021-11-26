using AutocompleteService.Messages.Events.Interfaces.IDomainEvent;
using System;
using System.Collections.Generic;

namespace AutocompleteService.Messages
{
    public class AggregateRoot
    {
        private readonly List<IDomainEvent> _events = new List<IDomainEvent>();
        public IEnumerable<IDomainEvent> Events => _events;

        protected void AddEvent(IDomainEvent @event)
        {
            _events.Add(@event);
        }
    }
}
