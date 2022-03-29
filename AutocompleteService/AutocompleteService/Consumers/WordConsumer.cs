using ApiGateway.Elasticsearch;
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

        protected override void LogMessageReceived(Word message)
        {
            ElkSearching.logger.Information($"Autocomplete service received message with text: {message.Text}");
        }

        protected override async Task SendEmail(Word message)
        {
            await Task.Delay(5000);
        }
    }
}
