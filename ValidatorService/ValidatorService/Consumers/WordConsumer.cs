using System;
using System.Threading.Tasks;
using ValidatorService.Models;

namespace ValidatorService.Consumers
{
    public class WordConsumer :
        AbstractConsumer<Word>
    {
        public WordConsumer(
            IServiceProvider serviceProvider
        ) : base(serviceProvider)
        {
            Queue = "Validator-service/word";

            _routingKey = "Validator-word";

            _exchange = "Validator-service";

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
