using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CacheService.Consumers
{
    public abstract class AbstractConsumer<T> : BackgroundService
    {
        protected IServiceProvider _serviceProvider { get; set; }
        protected IConnection _connection { get; set; }
        protected string Queue { get; set; }
        protected IModel _channel { get; set; }
        protected string _exchange { get; set; }
        protected string _routingKey { get; set; }

        public AbstractConsumer(
            IServiceProvider serviceProvider
        )
        {
            _serviceProvider = serviceProvider;
        }

        protected void InitializeEventBus()
        {
            var connectionFactory = new ConnectionFactory
            {
                HostName = "localhost",
                Port = 5672,
                UserName = "guest",
                Password = "guest"
            };

            _connection = connectionFactory.CreateConnection(
                "cache-service-consumer"
            );

            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(
                exchange: _exchange,
                type: ExchangeType.Topic,
                durable: false,
                autoDelete: false,
                arguments: null
            );
            _channel.QueueDeclare(
                queue: Queue,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );
            _channel.QueueBind(
                queue: Queue,
                exchange: _exchange,
                routingKey: _routingKey
            );
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (sender, eventArgs) =>
            {
                var contentArray = eventArgs.Body.ToArray();

                var contentString = Encoding.UTF8.GetString(contentArray);

                var message = JsonConvert
                    .DeserializeObject<T>(
                        contentString
                    );

                await RecieveSearchData(message);

                LogMessageReceived(message);

                _channel.BasicAck(eventArgs.DeliveryTag, false);
            };

            _channel.BasicConsume(
                queue: Queue,
                autoAck: false,
                consumer: consumer
            );

            return Task.CompletedTask;
        }

        protected abstract void LogMessageReceived(T message);

        protected abstract Task RecieveSearchData(T message);
    }
}
