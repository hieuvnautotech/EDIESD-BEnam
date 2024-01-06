using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace ESD_EDI_BE.RabbitMQ.PushToQueue
{
    public class RabbitMqPush
    {
        private static readonly object lockObject = new();
        private static IConnection? _connection;
        private static IModel? _channel;

        private static void CreateConnection()
        {
            if (_connection == null || !_connection.IsOpen)
            {
                var factory = new ConnectionFactory
                {
                    Uri = new Uri("amqp://evilhero:onconc1001@118.69.130.73:5672"),
                    ClientProvidedName = "Push to Queue",
                };
                _connection = factory.CreateConnection();
            }
        }

        public static IModel GetChannel()
        {
            lock (lockObject)
            {
                CreateConnection();
                if (_channel == null || _channel.IsClosed)
                {
                    _channel = _connection.CreateModel();
                }
                return _channel;
            }
        }

        public static void PublishToRabbitMQ(string exchangeName, string routingKey, string queueName, byte[] messageBody)
        {
            IModel channel = GetChannel();

            channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);
            channel.QueueDeclare(queueName, true, false, false, null);
            channel.QueueBind(queueName, exchangeName, routingKey, null);

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            channel.BasicPublish(exchangeName, routingKey, properties, messageBody);
            CloseConnection();
        }

        public static void CloseConnection()
        {
            if (_channel != null && _channel.IsOpen)
            {
                _channel.Close();
            }
            if (_connection != null && _connection.IsOpen)
            {
                _connection.Close();
            }
        }
    }
}