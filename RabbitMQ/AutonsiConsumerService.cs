using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ESD_EDI_BE.DbAccess;
using ESD_EDI_BE.Extensions;
using ESD_EDI_BE.Models;
using ESD_EDI_BE.Models.Dtos;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using static ESD_EDI_BE.Extensions.ServiceExtensions;

namespace ESD_EDI_BE.RabbitMQ
{
    public interface IAutonsiConsumerService
    {
        Task ReadMessages(string queueName);
    }

    public class AutonsiConsumerService : IAutonsiConsumerService, IDisposable
    {
        private readonly IRabbitMqService _rabbitMqService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly List<string> _queues;
        private readonly IModel _autonsiModel;
        private readonly IConnection _autonsiConnection;

        // private readonly ISqlDataAccess _sqlDataAccess;

        public AutonsiConsumerService(
            IWebHostEnvironment webHostEnvironment
            , IRabbitMqService rabbitMqService
            , List<string> queues
        // , ESD_DBContext esdDbContext
        // , ISqlDataAccess sqlDataAccess
        )
        {
            _webHostEnvironment = webHostEnvironment;
            _rabbitMqService = rabbitMqService;
            _queues = queues;
            _autonsiConnection = _rabbitMqService.CreateAutonsiChannel();
            _autonsiModel = _autonsiConnection.CreateModel();

            if (_webHostEnvironment.EnvironmentName == CommonConst.PRODUCTION)
            {
                foreach (var queue in _queues)
                {
                    _autonsiModel.QueueDeclare(queue: queue,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);
                    // var properties = _model.CreateBasicProperties();
                    // properties.Persistent = true;

                    _autonsiModel.ExchangeDeclare(exchange: CommonConst.AUTONSI_EXCHANGE,
                                            type: ExchangeType.Direct);

                    _autonsiModel.QueueBind(queue: queue,
                                        exchange: CommonConst.AUTONSI_EXCHANGE,
                                        routingKey: queue,
                                        arguments: null);
                }

            }
            // _esdDbContext = esdDbContext;
            // _sqlDataAccess = sqlDataAccess;
        }

        public async Task ReadMessages(string queueName)
        {
            var consumer = new AsyncEventingBasicConsumer(_autonsiModel);
            consumer.Received += async (ch, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = System.Text.Encoding.UTF8.GetString(body);

                switch (queueName)
                {
                    case CommonConst.AUTONSI_QUEUE_MACHINE:

                        await _rabbitMqService.ESDMachine(message);
                        await Task.CompletedTask;

                        _autonsiModel.BasicAck(deliveryTag: ea.DeliveryTag,
                                            multiple: false);
                        break;

                    case CommonConst.AUTONSI_QUEUE_MENU:

                        await _rabbitMqService.Menu(message);
                        await Task.CompletedTask;

                        _autonsiModel.BasicAck(deliveryTag: ea.DeliveryTag,
                                            multiple: false);
                        break;

                    case CommonConst.AUTONSI_QUEUE_MENU_PERMISSION:

                        await _rabbitMqService.MenuPermission(message);
                        await Task.CompletedTask;

                        _autonsiModel.BasicAck(deliveryTag: ea.DeliveryTag,
                                            multiple: false);
                        break;

                    case CommonConst.AUTONSI_QUEUE_DOCUMENT:

                        await _rabbitMqService.Document(message);
                        await Task.CompletedTask;

                        _autonsiModel.BasicAck(deliveryTag: ea.DeliveryTag,
                                            multiple: false);
                        break;

                    default:
                        break;
                }
            };
            _autonsiModel.BasicConsume(queueName, false, consumer);
            await Task.CompletedTask;
        }

        public void Dispose()
        {
            if (_autonsiModel != null && _autonsiModel.IsOpen) _autonsiModel.Close();
            if (_autonsiConnection != null && _autonsiConnection.IsOpen) _autonsiConnection.Close();
        }
    }
}