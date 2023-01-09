using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace RMQS.Application.Queue
{
    public interface IQueuePublisher
    {
        void Publish<T>(T message, bool throwError = false) where T : class;
    }

    public class QueuePublisher : IQueuePublisher
    {
        private readonly IConnectionFactory _factory;

        public QueuePublisher(IConnectionFactory factory)
        {
            _factory = factory;
        }

        public void Publish<T>(T message, bool throwError = false) where T : class
        {
            using (var connection = _factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                string serializedMessage = JsonConvert.SerializeObject(message);
                byte[] buffer = Encoding.UTF8.GetBytes(serializedMessage);

                var basicProperties = channel.CreateBasicProperties();
                basicProperties.Persistent = true;
                basicProperties.Headers = new Dictionary<string, object>()
                {
                    { "messageType", typeof(T).Name },
                    { "x-delay", 5000 }, // required by the delayed exchange
                    { "throwError", throwError ? 1 : 0 },
                };

                channel.ExchangeDeclare("my-main-exchange", ExchangeType.Fanout, true, false, null);

                channel.BasicPublish(
                    exchange: "my-main-exchange",
                    routingKey: "",
                    basicProperties: basicProperties,
                    body: buffer
                );
            }
        }
    }
}
