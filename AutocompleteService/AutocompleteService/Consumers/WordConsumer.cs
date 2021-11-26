using AutocompleteService.Models;
using System;
using System.Threading.Tasks;

namespace AutocompleteService.Consumers
{
    public class WordConsumer :
        AbstractConsumer<Word>
    {
        public WordConsumer(
            IServiceProvider serviceProvider
        ) : base(serviceProvider)
        {
            Queue = "autocomplete-service/word";

            _routingKey = "autocomplete-word";

            _exchange = "autocomplete-service";

            InitializeEventBus();
        }

        protected override void LogMessageReceived(
            Word message
        )
        {
            Console.WriteLine(
                $"Message UserCreated received with Id {message.Text}"
            );
        }

        protected override async Task SendEmail(Word message)
        {
            await Task.Delay(5000);

            Console.WriteLine("E-mail enviado para avisar sobre criação!");
        }
    }
}
