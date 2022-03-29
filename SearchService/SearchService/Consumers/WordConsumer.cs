using SearchService.Elasticsearch;
using SearchService.Models;
using System;
using System.Threading.Tasks;

namespace SearchService.Consumers
{
    public class WordConsumer :
        AbstractConsumer<Word>
    {
        public WordConsumer(
            IServiceProvider serviceProvider
        ) : base(serviceProvider)
        {
            Queue = "search-service/word";

            _routingKey = "search-word";

            _exchange = "search-service";

            InitializeEventBus();
        }

        protected override void LogMessageReceived(Word message)
        {
            Console.WriteLine(
                $"Message received with Id {message.Text}"
            );

            ElkSearching.logger.Information($"Search service received message with text {message.Text}");
        }

        protected override async Task SendEmail(Word message)
        {
            await Task.Delay(5000);
        }
    }
}
