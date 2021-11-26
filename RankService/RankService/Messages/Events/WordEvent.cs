using RankService.Events.Interfaces;
using System;

namespace RankService.Messages.Events
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
