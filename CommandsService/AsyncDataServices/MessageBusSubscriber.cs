namespace CommandsService.AsyncDataServices
{
    using System;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using CommandsService.EventProcessing;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;
    using Microsoft.Extensions.Configuration;

    public class MessageBusSubscriber : BackgroundService
    {
        private readonly IEventProcessor _eventProcessor;
        private IConnection _connection;
        private IModel _channel;
        private readonly IConfiguration _configuration;
        private const string ExchangeName = "trigger";
        private string _queueName;

        public MessageBusSubscriber( IConfiguration configuration,IEventProcessor eventProcessor)
        {
            _eventProcessor = eventProcessor;
            _configuration = configuration;

            InitializeRabbitMQListener();
        }

        private void InitializeRabbitMQListener()
        {
            var factory = new ConnectionFactory()
            {
                HostName = _configuration["RabbitMQHost"],
                Port = int.Parse(_configuration["RabbitMQPort"]),
                RequestedConnectionTimeout = TimeSpan.FromSeconds(30),
            };

            Console.WriteLine("==> Initializing RabbitMQ Listener...");
            Console.WriteLine($"==> RabbitMQ Host: {_configuration["RabbitMQHost"]} Port: {_configuration["RabbitMQPort"]}");

            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                _channel.ExchangeDeclare(exchange: ExchangeName, type: ExchangeType.Fanout, durable: true); 
                 _queueName = _channel.QueueDeclare().QueueName;
                _channel.QueueBind(queue: _queueName, exchange: ExchangeName, routingKey: string.Empty);

                Console.WriteLine("==> Connected to RabbitMQ and listening for messages.");

                _connection.ConnectionShutdown += OnConnectionShutdown;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not connect to RabbitMQ. " + ex.Message);
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (model, ea) =>
            {
                Console.WriteLine("--> Message received from RabbitMQ.");

                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                _eventProcessor.ProcessEvent(message);
            };

            _channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);

            return Task.CompletedTask;
        }

        private void OnConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            Console.WriteLine("#####  RabbitMQ connection shut down. ######");
        }

        public override void Dispose()
        {
            if (_channel.IsOpen)
            {
                _channel.Close();
                _connection.Close();
            }
            base.Dispose();
        }
    }
} 