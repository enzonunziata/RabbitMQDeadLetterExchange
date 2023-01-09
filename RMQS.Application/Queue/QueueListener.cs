using System.Text;
using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RMQS.Application.Hubs;

namespace RMQS.Application.Queue
{
    public class QueueListener : BackgroundService
    {
        private readonly IConnectionFactory _factory;
        private readonly IHubContext<RabbitMQHub> _hubContext;

        private IConnection _connection = default!;
        private IModel _channel = default!;

        public QueueListener(IConnectionFactory factory, IHubContext<RabbitMQHub> hubContext)
        {
            _factory = factory;
            _hubContext = hubContext;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare("my-main-exchange", ExchangeType.Fanout, true, false, null);
            _channel.ExchangeDeclare("my-dead-letter-exchange", "x-delayed-message", true, false, new Dictionary<string, object>
            {
                { "x-delayed-type", ExchangeType.Fanout }
            });

            _channel.QueueDeclare(
                queue: "my-main-queue",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object>()
                {
                    { "x-dead-letter-exchange", "my-dead-letter-exchange" }
                }
            );

            _channel.QueueDeclare("my-dead-letter-queue");

            _channel.QueueBind("my-main-queue", "my-main-exchange", "");
            _channel.QueueBind("my-dead-letter-queue", "my-dead-letter-exchange", "");

            return base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var mainConsumer = new EventingBasicConsumer(_channel);
            mainConsumer.Received += MainConsumerHandler;
            _channel.BasicConsume("my-main-queue", false, mainConsumer);

            var deadLetterConsumer = new EventingBasicConsumer(_channel);
            deadLetterConsumer.Received += DeadLetterConsumerHandler;
            _channel.BasicConsume("my-dead-letter-queue", false, deadLetterConsumer);

            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _channel.Close();
            _connection.Close();

            return base.StopAsync(cancellationToken);
        }

        private async void MainConsumerHandler(object? sender, BasicDeliverEventArgs e)
        {
            ExtractBody(e, out string messageType, out string body);
            bool throwError = ((int)e.BasicProperties.Headers["throwError"]) == 1;

            string context = string.Empty;
            if (throwError)
            {
                context = "main-failure";
                _channel.BasicNack(deliveryTag: e.DeliveryTag, multiple: false, requeue: false);
            }
            else
            {
                context = "main-success";
                _channel.BasicAck(deliveryTag: e.DeliveryTag, multiple: false);
            }

            await _hubContext.Clients.All.SendAsync("QueueReceived", context, messageType, body);
        }

        private async void DeadLetterConsumerHandler(object? sender, BasicDeliverEventArgs e)
        {
            ExtractBody(e, out string messageType, out string body);
            _channel.BasicAck(deliveryTag: e.DeliveryTag, multiple: false);
            await _hubContext.Clients.All.SendAsync("QueueReceived", "dead-letter", messageType, body);
        }

        private static void ExtractBody(BasicDeliverEventArgs e, out string messageType, out string body)
        {
            messageType = Encoding.UTF8.GetString((byte[])e.BasicProperties.Headers["messageType"]);
            body = Encoding.UTF8.GetString(e.Body.ToArray());
        }
    }
}
