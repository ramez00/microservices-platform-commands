using PlatformService.Dtos;
using PlatformService.AsyncDataServices;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace PlatformService.AsyncDataServices
{
    public class MessageBusClient : IMessageBusClient
    {
        private readonly IConfiguration _config;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        public MessageBusClient(IConfiguration config)
        {
            _config = config;
           var factory = new ConnectionFactory()
            {
                HostName = _config["RabbitMQHost"],
                Port = int.Parse(_config["RabbitMQPort"]),
                RequestedConnectionTimeout = TimeSpan.FromSeconds(30),  // Increase timeout
            };
            Console.WriteLine("==> Connecting to MessageBus...");

            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                _channel.ExchangeDeclare(exchange: "trigger", type: ExchangeType.Fanout, durable: true);
                
                _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;

                Console.WriteLine("==> Connected to MessageBus");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"==> Could not connect to the Message Bus: {ex.Message}");
                 throw; // 🔥 IMPORTANT

            }
        }
        public void PublishNewPlatform(PlatformPublishedDto platformPublishedDto)
        {
            Console.WriteLine("==> Publishing new platform to message bus");

            var message = JsonSerializer.Serialize(platformPublishedDto);

            if (_connection.IsOpen)
            {
                Console.WriteLine("==> RabbitMQ connection is open, sending message...");
                SendMessage(message);
            }
            else
                Console.WriteLine("==> RabbitMQ connection is closed, not sending");
            
        }

        private void SendMessage(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(
                exchange: "trigger",
                routingKey: "",            // fanout exchange ignores routing key
                basicProperties: null,
                body: body);

            Console.WriteLine($"==> We have sent {message}");
        }

        public void Dispose()
        {
            Console.WriteLine("==> MessageBus Disposed");
            if (_channel.IsOpen)
            {
                _channel.Close();
                _connection.Close();
            }
        }
         private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            Console.WriteLine("--> RabbitMQ Connection Shutdown");
        }
    }
}