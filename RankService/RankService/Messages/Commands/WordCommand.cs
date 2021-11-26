using RankService.Messages;
using RankService.Messages.Events;
using System;

namespace RankService.Messges.Commands
{
    public class WordCommand : AggregateRoot
    {
        private string word;
        public string Word
        {
            get 
            {
                return word;
            }
            set
            {
                word = value;                                                                           
                AddEvent(new WordEvent(Guid.NewGuid(), value));
            }
        }
    }
}
