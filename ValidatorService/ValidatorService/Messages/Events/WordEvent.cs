using System;
using Users.Domain.Events.Interfaces;

namespace ValidatorService.Messages.Events
{
    public class WordEvent : IDomainEvent {
        public WordEvent(Guid Id, string word)
        {
            this.Text = word;
            this.Id = Id;
        }

        public Guid Id { get; set; }

        public string Text { get; set; }
    }
}
