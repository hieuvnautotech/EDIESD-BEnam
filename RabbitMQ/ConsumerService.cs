using ESD_EDI_BE.Extensions;
using ESD_EDI_BE.Hubs;
using ESD_EDI_BE.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
//using static ESD_EDI_BE.Extensions.ServiceExtensions;

namespace ESD_EDI_BE.RabbitMQ
{
    public interface IConsumerService
    {
        Task ReadMessages(string queueName);
    }

    public class ConsumerService : IConsumerService, IDisposable
    {
        private readonly List<string> _queues;
        private readonly IModel _model;
        private readonly IConnection _connection;
        private readonly SignalRHub _signalRHub;
        private readonly ESD_DBContext _esdDbContext;

        public ConsumerService(IRabbitMqService rabbitMqService
                                , SignalRHub signalRHub
                                , List<string> queues
                                , ESD_DBContext esdDbContext)
        {
            _queues = queues;
            _connection = rabbitMqService.CreateChannel();
            _model = _connection.CreateModel();

            foreach (var queue in _queues)
            {
                _model.QueueDeclare(queue: queue,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
                // var properties = _model.CreateBasicProperties();
                // properties.Persistent = true;

                _model.ExchangeDeclare(exchange: CommonConst.EXCHANGE,
                                        type: ExchangeType.Direct);

                _model.QueueBind(queue: queue,
                                    exchange: CommonConst.EXCHANGE,
                                    routingKey: queue,
                                    arguments: null);
            }

            _signalRHub = signalRHub;
            // _serviceProvider = serviceProvider;
            _esdDbContext = esdDbContext;
        }

        public async Task ReadMessages(string queueName)
        {
            var consumer = new AsyncEventingBasicConsumer(_model);
            consumer.Received += async (ch, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = System.Text.Encoding.UTF8.GetString(body);

                switch (queueName)
                {
                    case CommonConst.QUEUE_MESSAGE:
                        // Push message to connected clients using SignalR
                        await _signalRHub.SendMessage(message);
                        await Task.CompletedTask;

                        _model.BasicAck(deliveryTag: ea.DeliveryTag,
                                            multiple: false);
                        break;

                    case CommonConst.QUEUE_PPORTAL_QUAL02_INFO:
                        await Save_PPORTAL_QUAL02_INFO(message);
                        await Task.CompletedTask;

                        _model.BasicAck(deliveryTag: ea.DeliveryTag,
                                            multiple: false);
                        break;

                    default:
                        break;
                }
            };
            _model.BasicConsume(queueName, false, consumer);
            await Task.CompletedTask;
        }

        private async Task Save_PPORTAL_QUAL02_INFO(string jsonList)
        {
            try
            {
                var data = JsonConvert.DeserializeObject<List<PPORTAL_QUAL02_INFO>>(jsonList);

                if (data != null)
                {
                    _esdDbContext.AddRange(data);

                    // Kiểm tra trạng thái trước khi lưu vào cơ sở dữ liệu
                    if (_esdDbContext.ChangeTracker.HasChanges())
                    {
                        await _esdDbContext.SaveChangesAsync();
                    }
                }
            }
            catch (JsonException jsonException)
            {
                // Xử lý lỗi chuyển đổi JSON
                Console.WriteLine($"Error deserializing JSON: {jsonException.Message}");
            }
            catch (DbUpdateException dbUpdateException)
            {
                // Xử lý lỗi cơ sở dữ liệu
                Console.WriteLine($"Database update error: {dbUpdateException.Message}");
            }
            catch (Exception ex)
            {
                // Xử lý các loại ngoại lệ khác
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            }
        }

        public void Dispose()
        {
            if (_model != null && _model.IsOpen) _model.Close();
            if (_connection != null && _connection.IsOpen) _connection.Close();
        }
    }
}
