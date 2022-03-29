using ValidatorService.Messages;
using ValidatorService.Messages.Events;
using System;

namespace ValidatorService.Messges.Commands
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
