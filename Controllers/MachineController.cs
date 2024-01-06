using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using ESD_EDI_BE.CustomAttributes;
using ESD_EDI_BE.Extensions;
using ESD_EDI_BE.Models.Dtos.Common;
using ESD_EDI_BE.Services.EDI;
using ESD_EDI_BE.Models.Dtos;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;
using ESD_EDI_BE.Services.Common;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using Newtonsoft.Json;
using ESD_EDI_BE.RabbitMQ.PushToQueue;
using ESD_EDI_BE.Models;
using ESD_EDI_BE.ElasticSearch.Services;
using ESD_EDI_BE.ElasticSearch.Services.Machine;

namespace ESD_EDI_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MachineController : ControllerBase
    {
        private readonly IES_MachineService _esMachineService;
        private readonly IMachineService _machineService;
        private readonly IJwtService _jwtService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public MachineController(
            IMachineService machineService
            , IJwtService jwtService
            , IWebHostEnvironment webHostEnvironment
             , IES_MachineService esMachineService
            )
        {
            _machineService = machineService;
            _jwtService = jwtService;
            _webHostEnvironment = webHostEnvironment;
            _esMachineService = esMachineService;
        }

        [HttpGet]
        [PermissionAuthorization(PermissionConst.MACHINE_READ)]
        public async Task<IActionResult> Get([FromQuery] MachineDto model)
        {
            return Ok(await _machineService.Get(model));
        }

        [HttpGet("elas")]
        [AllowAll]
        public async Task<IActionResult> GetByElasticSearch([FromQuery] MachineDto model)
        {
            var searchResults = await _esMachineService.Search(model.MachineName, model.isActived);
            var returnData = new ResponseModel<IEnumerable<MachineDto>?>();

            if (!searchResults.ToList().Any())
            {
                returnData.ResponseMessage = StaticReturnValue.NO_DATA;
                returnData.HttpResponseCode = 204;
            }
            else
            {
                returnData.Data = searchResults;
                returnData.TotalRow = searchResults.Count;
            }

            return Ok(returnData);
        }

        [HttpGet("get-by-code")]
        [PermissionAuthorization(PermissionConst.MACHINE_READ)]
        public async Task<IActionResult> GetByCode(string MachineCode)
        {
            return Ok(await _machineService.GetByCode(MachineCode));
        }

        [HttpPost("create")]
        [PermissionAuthorization(PermissionConst.MACHINE_CREATE)]
        public async Task<IActionResult> Create([FromBody] MachineDto model)
        {
            var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var userId = await _jwtService.ValidateTokenAsync(token);
            model.createdBy = long.Parse(userId);
            model.createdName = _jwtService.GetUserNameFromToken(token);
            model.MachineId = AutoId.AutoGenerate();
            var result = await _machineService.Create(model);

            await _esMachineService.Insert(result.Data);
            // await Task.Delay(500);

            if (_webHostEnvironment.EnvironmentName == CommonConst.STAGING)
            {
                result.Data.RabbitMQType = CommonConst.RABBITMQ_TYPE_CREATE;
                byte[] messageBody = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(result.Data));
                RabbitMqPush.PublishToRabbitMQ(exchangeName: CommonConst.AUTONSI_EXCHANGE,
                                                                           routingKey: CommonConst.AUTONSI_QUEUE_MACHINE,
                                                                           queueName: CommonConst.AUTONSI_QUEUE_MACHINE,
                                                                           messageBody: messageBody);
            }

            return Ok(result);
        }

        [HttpPut("update")]
        [PermissionAuthorization(PermissionConst.MACHINE_UPDATE)]
        public async Task<IActionResult> Update([FromBody] MachineDto model)
        {
            var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var userId = await _jwtService.ValidateTokenAsync(token);
            model.modifiedBy = long.Parse(userId);
            model.modifiedName = _jwtService.GetUserNameFromToken(token);
            var result = await _machineService.Modify(model);

            if (result.HttpResponseCode == 200)
            {
                await _esMachineService.Update(result.Data);
                // await Task.Delay(500);

                if (_webHostEnvironment.EnvironmentName == CommonConst.STAGING)
                {
                    result.Data.RabbitMQType = CommonConst.RABBITMQ_TYPE_UPDATE;
                    byte[] messageBody = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(result.Data));
                    RabbitMqPush.PublishToRabbitMQ(exchangeName: CommonConst.AUTONSI_EXCHANGE,
                                                                               routingKey: CommonConst.AUTONSI_QUEUE_MACHINE,
                                                                               queueName: CommonConst.AUTONSI_QUEUE_MACHINE,
                                                                               messageBody: messageBody);
                }
            }

            return Ok(result);
        }

        [HttpPut("delete")]
        [PermissionAuthorization(PermissionConst.MACHINE_DELETE)]
        public async Task<IActionResult> Delete([FromBody] MachineDto model)
        {
            var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var userId = await _jwtService.ValidateTokenAsync(token);
            model.modifiedBy = long.Parse(userId);
            var result = await _machineService.Delete(model);

            var deleteMachine = await _machineService.GetById(model.MachineId);
            await _esMachineService.Update(deleteMachine.Data);
            // await Task.Delay(500);

            if (result.HttpResponseCode == 200 && _webHostEnvironment.EnvironmentName == CommonConst.STAGING)
            {

                deleteMachine.Data.RabbitMQType = CommonConst.RABBITMQ_TYPE_DELETE;
                byte[] messageBody = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(deleteMachine.Data));
                RabbitMqPush.PublishToRabbitMQ(exchangeName: CommonConst.AUTONSI_EXCHANGE,
                                                                           routingKey: CommonConst.AUTONSI_QUEUE_MACHINE,
                                                                           queueName: CommonConst.AUTONSI_QUEUE_MACHINE,
                                                                           messageBody: messageBody);
            }

            return Ok(result);
        }
    }
}
