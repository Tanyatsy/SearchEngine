using System;
using System.Text;
using SearchService.Elasticsearch;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RabbitMQ.Client;

namespace SearchService.MessageBus
{
    public class RabbitMQClient : IMessageBusClient
    {
        private readonly IConnection _connection;

        public RabbitMQClient()
        {
            var connectionFactory = new ConnectionFactory {
                HostName = "rabbitmq",
                UserName = "guest",
                Password = "guest"
            };

            _connection = connectionFactory.CreateConnection(
                "search-service-producer"
                );
        }

        public void Publish(object message, string routingKey, string exchange)
        {
            var channel = _connection.CreateModel();

            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            var payload = JsonConvert.SerializeObject(message, settings);

            Console.WriteLine(payload);

            var body = Encoding.UTF8.GetBytes(payload);

            channel.ExchangeDeclare(
                exchange: exchange,
                type: ExchangeType.Topic,
                durable: false
            );

            Console.WriteLine($"{exchange}->{routingKey}");

            ElkSearching.logger.Information($"{exchange}->{routingKey}");

            channel.BasicPublish(
                exchange: exchange,
                routingKey: routingKey,
                basicProperties: null,
                body: body
            );
        }
    }

    public interface IMessageBusClient
    {
        void Publish(object message, string routingKey, string exchange);
    }
}
