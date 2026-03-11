using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Threading.Channels;

namespace BusinessLogicLayer.RabbitMQ
{
    public class RabbitMQPublisher : IRabbitMQPublisher, IDisposable
    {
        private readonly IConfiguration _configuration;
        private IConnection _connection;
        private IChannel _channel;
        private readonly ILogger<RabbitMQPublisher> _logger;
        public RabbitMQPublisher(IConfiguration configuration, ILogger<RabbitMQPublisher> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }
        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }

        //public async Task Publish<T>(string routingKey, T message)
        public async Task Publish<T>(Dictionary<string,object> headers, T message) //167

        {
            try
            {
                //_logger.LogInformation("RabbitMQPublisher.Publish START, routingKey={routingKey}", routingKey);
                string hostName = _configuration["RABBITMQ_HOST"]!;
                string userName = _configuration["RABBITMQ_USERNAME"]!;
                string password = _configuration["RABBITMQ_PASSWORD"]!;
                string port = _configuration["RABBITMQ_PORT"]!;

                ConnectionFactory connectionFactory = new ConnectionFactory
                {
                    HostName = hostName,
                    UserName = userName,
                    Password = password,
                    Port = Convert.ToInt32(port)
                }; //158
                _logger.LogInformation("RabbitMQ config host={hostName} port={port} user={userName}",
    hostName, port, userName);
                _connection = await connectionFactory.CreateConnectionAsync();

                _channel = await _connection.CreateChannelAsync(); //158
                _logger.LogInformation("Connection + channel created");

                string messageJson = System.Text.Json.JsonSerializer.Serialize(message);
                byte[] messageBodyInBytes = System.Text.Encoding.UTF8.GetBytes(messageJson);
                //string exchangeName = "products.exchange"; // specify exchange name if needed
                string exchangeName = _configuration["RABBITMQ_PRODUCTS_EXCHANGE"]!;
                
                _logger.LogInformation("Declaring exchange {exchange}", exchangeName);
                // If the exchange already exists, this will do nothing. If it doesn't exist, it will be created.
                await _channel.ExchangeDeclareAsync(exchange: exchangeName, type: ExchangeType.Headers, durable: true); //159 //165 //166 //167
                _logger.LogInformation("Published message to {exchange} / {routingKey}", exchangeName, string.Empty); //167

                var props = new BasicProperties();
                props.ContentType = "application/json";
                props.DeliveryMode = DeliveryModes.Persistent; // make it durable
                                                               // set props if needed
                                                               props.Headers = headers; //167
                // Publish the message to the exchange with the specified routing key
                await _channel.BasicPublishAsync(
                    exchange: exchangeName,
                    routingKey: string.Empty, //166 //167 - routing key is ignored for headers exchange
                    mandatory: true,
                    basicProperties: props,
                    body: messageBodyInBytes); //159
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while publishing to RabbitMQ");
                throw; // or decide how to handle
            }
        }
    }
}
