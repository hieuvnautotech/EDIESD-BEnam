using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using ESD_EDI_BE.CustomAttributes;
using ESD_EDI_BE.Extensions;
using ESD_EDI_BE.Models.Dtos.Common;
using ESD_EDI_BE.Services.Common;
using ESD_EDI_BE.Services.Cache;
using ESD_EDI_BE.Hubs;
using ESD_EDI_BE.RabbitMQ.PushToQueue;
using System.Text;
using Newtonsoft.Json;

namespace ESD_EDI_BE.Controllers.Standard.Configuration
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuController : ControllerBase
    {
        private readonly IMenuService _menuService;
        private readonly IJwtService _jwtService;
        private readonly IUserService _userService;
        private readonly ISysCacheService _sysCacheService;
        private readonly SignalRHub _signalRHub;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public MenuController(
            IMenuService menuService
            , IUserService userService
            , IJwtService jwtService
            , ISysCacheService sysCacheService
            , SignalRHub signalRHub
            , IWebHostEnvironment webHostEnvironment
        )
        {
            _menuService = menuService;
            _userService = userService;
            _jwtService = jwtService;
            _sysCacheService = sysCacheService;
            _signalRHub = signalRHub;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        [PermissionAuthorization(PermissionConst.MENU_READ)]
        public async Task<IActionResult> Get([FromQuery] MenuDto model, string? keyWord)
        {
            var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var userId = await _jwtService.ValidateTokenAsync(token);
            var userRole = await _userService.GetUserRole(long.Parse(userId));
            if (userRole.Contains(RoleConst.ROOT))
            {
                return Ok(await _menuService.Get(model, keyWord));
            }
            else
            {
                return Ok(await _menuService.GetExceptRoot(model, keyWord));
            }
        }

        [HttpPost("create-menu")]
        [RoleAuthorization(RoleConst.ROOT)]
        [PermissionAuthorization(PermissionConst.MENU_CREATE)]
        public async Task<IActionResult> Create([FromBody] MenuDto model)
        {
            var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var userId = await _jwtService.ValidateTokenAsync(token);
            model.createdBy = long.Parse(userId);
            model.menuId = AutoId.AutoGenerate();
            if (model.parentId == 0)
            {
                model.parentId = null;
            }

            model.sortOrder ??= 0;

            var result = await _menuService.Create(model);

            var returnData = new ResponseModel<MenuDto?>()
            {
                ResponseMessage = result
            };
            switch (result)
            {
                case StaticReturnValue.SYSTEM_ERROR:
                    returnData.HttpResponseCode = 500;
                    break;
                case StaticReturnValue.SUCCESS:
                    if (model.forRoot)
                    {
                        await _sysCacheService.SetRoleMenusToRedis(RoleConst.ROOT);
                        await _signalRHub.SendRoleMenusUpdate(RoleConst.ROOT);
                    }
                    else
                    {
                        await _sysCacheService.SetRoleMenusToRedis(RoleConst.ROOT);
                        await _sysCacheService.SetRoleMenusToRedis(RoleConst.ADMIN);
                        await _signalRHub.SendRoleMenusUpdate(RoleConst.ROOT);
                        await _signalRHub.SendRoleMenusUpdate(RoleConst.ADMIN);
                    }

                    returnData = await _menuService.GetById(model.menuId);
                    returnData.ResponseMessage = result;

                    if (_webHostEnvironment.EnvironmentName == CommonConst.STAGING)
                    {

                        returnData.Data.RabbitMQType = CommonConst.RABBITMQ_TYPE_CREATE;
                        byte[] messageBody = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(returnData.Data));
                        RabbitMqPush.PublishToRabbitMQ(exchangeName: CommonConst.AUTONSI_EXCHANGE,
                                                                                   routingKey: CommonConst.AUTONSI_QUEUE_MENU,
                                                                                   queueName: CommonConst.AUTONSI_QUEUE_MENU,
                                                                                   messageBody: messageBody);
                    }
                    break;
                default:
                    returnData.HttpResponseCode = 400;
                    break;
            }
            return Ok(returnData);
        }

        [HttpPut("modify-menu")]
        [RoleAuthorization(RoleConst.ROOT)]
        [PermissionAuthorization(PermissionConst.MENU_UPDATE)]
        public async Task<IActionResult> Modify([FromBody] MenuDto model)
        {
            if (model.parentId == 0)
            {
                model.parentId = null;
            }

            var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var userId = await _jwtService.ValidateTokenAsync(token);
            model.modifiedBy = long.Parse(userId);
            var result = await _menuService.Modify(model);

            var returnData = new ResponseModel<MenuDto?>()
            {
                ResponseMessage = result,
            };
            switch (result)
            {
                case StaticReturnValue.SYSTEM_ERROR:
                    returnData.HttpResponseCode = 500;
                    break;
                case StaticReturnValue.SUCCESS:
                    await _sysCacheService.SetRoleMenusToRedis();
                    await _signalRHub.SendRoleMenusUpdate(RoleConst.ROOT);
                    await _signalRHub.SendRoleMenusUpdate(RoleConst.ADMIN);

                    returnData = await _menuService.GetById(model.menuId);
                    returnData.ResponseMessage = result;

                    if (_webHostEnvironment.EnvironmentName == CommonConst.STAGING)
                    {

                        returnData.Data.RabbitMQType = CommonConst.RABBITMQ_TYPE_UPDATE;
                        byte[] messageBody = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(returnData.Data));
                        RabbitMqPush.PublishToRabbitMQ(exchangeName: CommonConst.AUTONSI_EXCHANGE,
                                                                                   routingKey: CommonConst.AUTONSI_QUEUE_MENU,
                                                                                   queueName: CommonConst.AUTONSI_QUEUE_MENU,
                                                                                   messageBody: messageBody);
                    }
                    break;
                default:
                    returnData.HttpResponseCode = 400;
                    break;
            }

            return Ok(returnData);
        }

        [HttpDelete("delete-menu")]
        [RoleAuthorization(RoleConst.ROOT)]
        [PermissionAuthorization(PermissionConst.MENU_DELETE)]
        public async Task<IActionResult> Delete([FromBody] MenuDto model)
        {
            var menu = await _menuService.GetById(model.menuId);
            var result = await _menuService.Delete(model);

            var returnData = new ResponseModel<MenuDto?>()
            {
                ResponseMessage = result,
                Data = menu.Data
            };
            switch (result)
            {
                case StaticReturnValue.SYSTEM_ERROR:
                    returnData.HttpResponseCode = 500;
                    break;
                case StaticReturnValue.SUCCESS:
                    await _sysCacheService.SetRoleMenusToRedis();
                    await _signalRHub.SendRoleMenusUpdate(RoleConst.ROOT);
                    await _signalRHub.SendRoleMenusUpdate(RoleConst.ADMIN);

                    if (_webHostEnvironment.EnvironmentName == CommonConst.STAGING)
                    {

                        returnData.Data.RabbitMQType = CommonConst.RABBITMQ_TYPE_DELETE;
                        byte[] messageBody = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(returnData.Data));
                        RabbitMqPush.PublishToRabbitMQ(exchangeName: CommonConst.AUTONSI_EXCHANGE,
                                                                                   routingKey: CommonConst.AUTONSI_QUEUE_MENU,
                                                                                   queueName: CommonConst.AUTONSI_QUEUE_MENU,
                                                                                   messageBody: messageBody);
                    }
                    break;
                default:
                    returnData.HttpResponseCode = 400;
                    break;
            }

            return Ok(returnData);
        }

        [HttpGet("get-by-level")]
        [PermissionAuthorization(PermissionConst.MENU_READ)]
        public async Task<IActionResult> GetByLevel(byte menuLevel)
        {
            return Ok(await _menuService.GetByLevel(menuLevel));
        }

        [HttpGet("get-menu-permission")]
        [PermissionAuthorization(PermissionConst.MENU_READ)]
        public async Task<IActionResult> GetMenuPermission(long menuId)
        {
            var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var userId = await _jwtService.ValidateTokenAsync(token);

            var userRole = await _userService.GetUserRole(long.Parse(userId));
            return Ok(await _menuService.GetMenuPermission(menuId, (IList<string>)userRole));
        }

        [HttpPost("create-menu-permission")]
        [RoleAuthorization(RoleConst.ROOT)]
        [PermissionAuthorization(PermissionConst.MENU_SET_PERMISSION)]
        public async Task<IActionResult> CreateMenuPermission([FromBody] MenuPermissionDto model)
        {
            var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var userId = await _jwtService.ValidateTokenAsync(token);
            model.createdBy = long.Parse(userId);
            model.Id = AutoId.AutoGenerate();

            var result = await _menuService.CreateMenuPermission(model);

            var returnData = new ResponseModel<MenuPermissionDto>()
            {
                ResponseMessage = result
            };
            switch (result)
            {
                case StaticReturnValue.SYSTEM_ERROR:
                    returnData.HttpResponseCode = 500;
                    break;
                case StaticReturnValue.SUCCESS:
                    await _sysCacheService.SetRoleMenuPermissionsToRedis();

                    if (model.forRoot == false || model.forRoot == null)
                    {
                        await _signalRHub.SendUpdateRoleMissingPermissions(RoleConst.ADMIN);
                    }

                    returnData = await _menuService.GetMenuPermissionById(model.Id);

                    returnData.ResponseMessage = result;
                    if (_webHostEnvironment.EnvironmentName == CommonConst.STAGING)
                    {

                        returnData.Data.RabbitMQType = CommonConst.RABBITMQ_TYPE_CREATE;
                        byte[] messageBody = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(returnData.Data));
                        RabbitMqPush.PublishToRabbitMQ(exchangeName: CommonConst.AUTONSI_EXCHANGE,
                                                                                   routingKey: CommonConst.AUTONSI_QUEUE_MENU_PERMISSION,
                                                                                   queueName: CommonConst.AUTONSI_QUEUE_MENU_PERMISSION,
                                                                                   messageBody: messageBody);
                    }
                    break;
                default:
                    returnData.HttpResponseCode = 400;
                    break;
            }

            return Ok(returnData);
        }

        [HttpPut("modify-menu-permission")]
        [RoleAuthorization(RoleConst.ROOT)]
        [PermissionAuthorization(PermissionConst.MENU_UPDATE)]
        public async Task<IActionResult> ModifyMenuPermission([FromBody] MenuPermissionDto model)
        {
            var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var userId = await _jwtService.ValidateTokenAsync(token);

            model.modifiedBy = long.Parse(userId);

            var result = await _menuService.ModifyMenuPermission(model);

            var returnData = new ResponseModel<MenuPermissionDto>()
            {
                ResponseMessage = result,
                Data = model
            };
            switch (result)
            {
                case StaticReturnValue.SYSTEM_ERROR:
                    returnData.HttpResponseCode = 500;
                    break;
                case StaticReturnValue.SUCCESS:
                    await _sysCacheService.SetRoleMenuPermissionsToRedis();
                    await _sysCacheService.SetRoleMissingMenuPermissionsToRedis();

                    await _signalRHub.SendUpdateRoleMissingPermissions(RoleConst.ADMIN);

                    // returnData = await _menuService.GetMenuPermissionById(model.Id);
                    if (_webHostEnvironment.EnvironmentName == CommonConst.STAGING)
                    {

                        model.RabbitMQType = CommonConst.RABBITMQ_TYPE_UPDATE;
                        byte[] messageBody = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(model));
                        RabbitMqPush.PublishToRabbitMQ(exchangeName: CommonConst.AUTONSI_EXCHANGE,
                                                                                   routingKey: CommonConst.AUTONSI_QUEUE_MENU_PERMISSION,
                                                                                   queueName: CommonConst.AUTONSI_QUEUE_MENU_PERMISSION,
                                                                                   messageBody: messageBody);
                    }
                    returnData.ResponseMessage = result;
                    break;
                default:
                    returnData.HttpResponseCode = 400;
                    break;
            }

            return Ok(returnData);
        }

        [HttpDelete("delete-menu-permission/{Id}")]
        [RoleAuthorization(RoleConst.ROOT)]
        [PermissionAuthorization(PermissionConst.MENU_DELETE)]
        public async Task<IActionResult> DeleteMenuPermission(long Id)
        {
            var result = await _menuService.DeleteMenuPermission(Id);

            var returnData = new ResponseModel<MenuPermissionDto>()
            {
                ResponseMessage = result
            };
            switch (result)
            {
                case StaticReturnValue.SYSTEM_ERROR:
                    returnData.HttpResponseCode = 500;
                    break;
                case StaticReturnValue.SUCCESS:
                    await _sysCacheService.SetRoleMenuPermissionsToRedis();

                    await _signalRHub.SendUpdateRoleMissingPermissions(RoleConst.ADMIN);

                    if (_webHostEnvironment.EnvironmentName == CommonConst.STAGING)
                    {

                        returnData.Data = new MenuPermissionDto()
                        {
                            Id = Id,
                            RabbitMQType = CommonConst.RABBITMQ_TYPE_DELETE
                        };
                        byte[] messageBody = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(returnData.Data));
                        RabbitMqPush.PublishToRabbitMQ(exchangeName: CommonConst.AUTONSI_EXCHANGE,
                                                                                   routingKey: CommonConst.AUTONSI_QUEUE_MENU_PERMISSION,
                                                                                   queueName: CommonConst.AUTONSI_QUEUE_MENU_PERMISSION,
                                                                                   messageBody: messageBody);
                    }
                    break;
                default:
                    returnData.HttpResponseCode = 400;
                    break;
            }

            return Ok(returnData);
        }
    }
}
