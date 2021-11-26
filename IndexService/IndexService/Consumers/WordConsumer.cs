using System;
using System.Threading.Tasks;
using IndexService.Models;

namespace IndexService.Consumers
{
    public class WordConsumer :
        AbstractConsumer<Word>
    {
        public WordConsumer(
            IServiceProvider serviceProvider
        ) : base(serviceProvider)
        {
            Queue = "index-service/word";

            _routingKey = "index-word";

            _exchange = "index-service";

            InitializeEventBus();
        }

        protected override void LogMessageReceived(
            Word message
        )
        {
            Console.WriteLine(
                $"Message received with Text {message.Text}"
            );
        }

        protected override async Task SendPageData(Word message)
        {
          
        }
    }
}
