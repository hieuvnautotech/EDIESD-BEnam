using System.Data;
using Dapper;
using ESD_EDI_BE.DbAccess;
using ESD_EDI_BE.ElasticSearch.Services;
using ESD_EDI_BE.ElasticSearch.Services.Machine;
using ESD_EDI_BE.Extensions;
using ESD_EDI_BE.Hubs;
using ESD_EDI_BE.Models;
using ESD_EDI_BE.Models.Dtos;
using ESD_EDI_BE.Models.Dtos.Common;
using ESD_EDI_BE.Services.Cache;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using static ESD_EDI_BE.Extensions.ServiceExtensions;

namespace ESD_EDI_BE.RabbitMQ
{
    public interface IRabbitMqService
    {
        #region Channel
        IConnection CreateChannel();
        IConnection CreateAutonsiChannel();
        #endregion

        #region MENU
        Task Menu(string rabbitMqMessage);
        #endregion

        #region MENU_PERMISSION
        Task MenuPermission(string rabbitMqMessage);
        #endregion

        #region DOCUMENT
        Task Document(string rabbitMqMessage);
        #endregion

        #region MACHINE
        Task ESDMachine(string rabbitMqMessage);
        #endregion
    }

    [SingletonRegistration]
    public class RabbitMqService : IRabbitMqService
    {
        private readonly RabbitMqConfiguration _configuration;
        private readonly AutonsiRabbitMqConfiguration _autonsiConfiguration;
        private readonly ESD_DBContext _esdDbContext;
        private readonly ISqlDataAccess _sqlDataAccess;
        private readonly ISysCacheService _sysCacheService;
        private readonly SignalRHub _signalRHub;
        private readonly IES_MachineService _esMachineService;

        public RabbitMqService
        (
            IOptions<RabbitMqConfiguration> options
            , IOptions<AutonsiRabbitMqConfiguration> autonsiOptions
            , ESD_DBContext esdDbContext
            , ISqlDataAccess sqlDataAccess
            , ISysCacheService sysCacheService
            , SignalRHub signalRHub
            , IES_MachineService esMachineService
        )
        {
            _configuration = options.Value;
            _autonsiConfiguration = autonsiOptions.Value;
            _esdDbContext = esdDbContext;
            _sqlDataAccess = sqlDataAccess;
            _sysCacheService = sysCacheService;
            _signalRHub = signalRHub;
            _esMachineService = esMachineService;
        }

        #region Channel
        public IConnection CreateChannel()
        {

            var factory = new ConnectionFactory
            {
                UserName = _configuration.Username,
                Password = _configuration.Password,
                HostName = _configuration.HostName,
                ClientProvidedName = "Files Monitor Receiver App",
                DispatchConsumersAsync = true
            };
            try
            {
                var channel = factory.CreateConnection();
                return channel;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public IConnection CreateAutonsiChannel()
        {
            var autonsiFactory = new ConnectionFactory
            {
                UserName = _autonsiConfiguration.Username,
                Password = _autonsiConfiguration.Password,
                HostName = _autonsiConfiguration.HostName,
                ClientProvidedName = "Receiver from Autonsi",
                DispatchConsumersAsync = true
            };
            try
            {
                var autonsiChannel = autonsiFactory.CreateConnection();
                return autonsiChannel;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        #endregion

        #region MENU
        public async Task Menu(string rabbitMqMessage)
        {
            try
            {
                var data = JsonConvert.DeserializeObject<MenuDto>(rabbitMqMessage);
                Console.WriteLine("RabbitMQ Menu Data: " + rabbitMqMessage);
                string returnMessage = string.Empty;
                if (data != null)
                {
                    switch (data.RabbitMQType)
                    {
                        case CommonConst.RABBITMQ_TYPE_UPDATE:
                            var updateItem = await GetMenuById(data.menuId);
                            data.row_version = updateItem.row_version;
                            returnMessage = await ModifyMenu(data);
                            Console.WriteLine("Modify Menu Message: " + returnMessage);
                            if (returnMessage == StaticReturnValue.SUCCESS)
                            {
                                await _sysCacheService.SetRoleMenusToRedis();
                                await _signalRHub.SendRoleMenusUpdate(RoleConst.ROOT);
                                await _signalRHub.SendRoleMenusUpdate(RoleConst.ADMIN);
                            }
                            break;

                        case CommonConst.RABBITMQ_TYPE_DELETE:
                            returnMessage = await DeleteMenu(data);
                            Console.WriteLine("Delete Menu Message: " + returnMessage);
                            if (returnMessage == StaticReturnValue.SUCCESS)
                            {
                                await _sysCacheService.SetRoleMenusToRedis();
                                await _signalRHub.SendRoleMenusUpdate(RoleConst.ROOT);
                                await _signalRHub.SendRoleMenusUpdate(RoleConst.ADMIN);
                            }
                            break;

                        default:
                            returnMessage = await CreateMenu(data);
                            Console.WriteLine("Create Menu Message: " + returnMessage);
                            if (returnMessage == StaticReturnValue.SUCCESS)
                            {
                                if (data.forRoot)
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
                            }
                            break;
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

        private async Task<MenuDto> GetMenuById(long menuId)
        {
            string proc = "sysUsp_Menu_GetById";
            var param = new DynamicParameters();
            param.Add("@menuId", menuId);

            var data = await _sqlDataAccess.LoadDataUsingStoredProcedure<MenuDto>(proc, param);
            return data.FirstOrDefault();
        }

        private async Task<string> CreateMenu(MenuDto model)
        {
            string proc = "sysUsp_Menu_Create";
            var param = new DynamicParameters();
            param.Add("@menuId", model.menuId);
            param.Add("@parentId", model.parentId);
            param.Add("@menuName", model.menuName);
            param.Add("@menuIcon", model.menuIcon);
            param.Add("@languageKey", model.languageKey);
            param.Add("@menuComponent", model.menuComponent);
            param.Add("@navigateUrl", model.navigateUrl);
            param.Add("@forRoot", model.forRoot);
            param.Add("@forApp", model.forApp);
            param.Add("@menuLevel", model.menuLevel);
            param.Add("@sortOrder", model.sortOrder);
            param.Add("@createdBy", model.createdBy);
            param.Add("@output", dbType: DbType.String, direction: ParameterDirection.Output, size: int.MaxValue);
            return await _sqlDataAccess.SaveDataUsingStoredProcedure<string>(proc, param);
        }

        private async Task<string> ModifyMenu(MenuDto model)
        {
            string proc = "sysUsp_Menu_Modify";
            var param = new DynamicParameters();
            param.Add("@menuId", model.menuId);
            param.Add("@parentId", model.parentId);
            param.Add("@menuName", model.menuName);
            param.Add("@menuLevel", model.menuLevel);
            param.Add("@sortOrder", model.sortOrder);
            param.Add("@menuIcon", model.menuIcon);
            param.Add("@languageKey", model.languageKey);
            param.Add("@menuComponent", model.menuComponent);
            param.Add("@navigateUrl", model.navigateUrl);
            param.Add("@forRoot", model.forRoot);
            param.Add("@forApp", model.forApp);
            param.Add("@modifyBy", model.modifiedBy);
            param.Add("@row_version", model.row_version);
            param.Add("@output", dbType: DbType.String, direction: ParameterDirection.Output, size: int.MaxValue);//luôn để DataOutput trong stored procedure

            return await _sqlDataAccess.SaveDataUsingStoredProcedure<int>(proc, param);
        }

        private async Task<string> DeleteMenu(MenuDto model)
        {
            string proc = "sysUsp_Menu_Delete";
            var param = new DynamicParameters();
            param.Add("@menuId", model.menuId);

            param.Add("@output", dbType: DbType.String, direction: ParameterDirection.Output, size: int.MaxValue);//luôn để DataOutput trong stored procedure

            return await _sqlDataAccess.SaveDataUsingStoredProcedure<int>(proc, param);
        }
        #endregion

        #region MENU_PERMISSION
        public async Task MenuPermission(string rabbitMqMessage)
        {
            try
            {
                var data = JsonConvert.DeserializeObject<MenuPermissionDto>(rabbitMqMessage);
                Console.WriteLine("RabbitMQ Menu Permission Data: " + rabbitMqMessage);
                string returnMessage = string.Empty;

                if (data != null)
                {
                    switch (data.RabbitMQType)
                    {
                        case CommonConst.RABBITMQ_TYPE_UPDATE:
                            var updateItem = await GetMenuPermissionById(data.Id);
                            data.row_version = updateItem.row_version;
                            returnMessage = await ModifyMenuPermission(data);
                            Console.WriteLine("Modify Menu Permission Message: " + returnMessage);
                            if (returnMessage == StaticReturnValue.SUCCESS)
                            {
                                await _sysCacheService.SetRoleMenuPermissionsToRedis();
                                await _sysCacheService.SetRoleMissingMenuPermissionsToRedis();

                                await _signalRHub.SendUpdateRoleMissingPermissions(RoleConst.ADMIN);
                            }
                            break;

                        case CommonConst.RABBITMQ_TYPE_DELETE:
                            returnMessage = await DeleteMenuPermission(data.Id);
                            Console.WriteLine("Delete Menu Permission Message: " + returnMessage);
                            if (returnMessage == StaticReturnValue.SUCCESS)
                            {
                                await _sysCacheService.SetRoleMenuPermissionsToRedis();

                                await _signalRHub.SendUpdateRoleMissingPermissions(RoleConst.ADMIN);
                            }
                            break;

                        default:
                            returnMessage = await CreateMenuPermission(data);
                            Console.WriteLine("Create Menu Permission Message: " + returnMessage);
                            if (returnMessage == StaticReturnValue.SUCCESS)
                            {
                                await _sysCacheService.SetRoleMenuPermissionsToRedis();
                                if (data.forRoot == false || data.forRoot == null)
                                {
                                    await _signalRHub.SendUpdateRoleMissingPermissions(RoleConst.ADMIN);
                                }
                            }
                            break;
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

        private async Task<MenuPermissionDto> GetMenuPermissionById(long id)
        {
            string proc = "sysUsp_Menu_GetMenuPermissionById";
            var param = new DynamicParameters();
            param.Add("@Id", id);

            var data = await _sqlDataAccess.LoadDataUsingStoredProcedure<MenuPermissionDto>(proc, param);
            return data.FirstOrDefault();
        }

        private async Task<string> CreateMenuPermission(MenuPermissionDto model)
        {
            string proc = "sysUsp_Menu_CreateMenuPermission";
            var param = new DynamicParameters();
            param.Add("@Id", model.Id);
            param.Add("@MP_Name", model.MP_Name.Trim());
            param.Add("@MP_Description", model.MP_Description.Trim());
            param.Add("@photo", model.photo.Trim());
            param.Add("@menuId", model.menuId);
            param.Add("@forRoot", model.forRoot);
            param.Add("@createdBy", model.createdBy);
            param.Add("@output", dbType: DbType.String, direction: ParameterDirection.Output, size: int.MaxValue);//luôn để DataOutput trong stored procedure

            return await _sqlDataAccess.SaveDataUsingStoredProcedure<string>(proc, param);
        }

        private async Task<string> ModifyMenuPermission(MenuPermissionDto model)
        {
            string proc = "sysUsp_Menu_ModifyMenuPermission";
            var param = new DynamicParameters();
            param.Add("@Id", model.Id);
            param.Add("@MP_Name", model.MP_Name);
            param.Add("@MP_Description", model.MP_Description);
            param.Add("@photo", model.photo.Trim());
            param.Add("@forRoot", model.forRoot);
            param.Add("@modifiedBy", model.modifiedBy);
            param.Add("@row_version", model.row_version);
            param.Add("@output", dbType: DbType.String, direction: ParameterDirection.Output, size: int.MaxValue);//luôn để DataOutput trong stored procedure

            return await _sqlDataAccess.SaveDataUsingStoredProcedure<string>(proc, param);
        }

        private async Task<string> DeleteMenuPermission(long Id)
        {
            string proc = "sysUsp_Menu_DeleteMenuPermission";
            var param = new DynamicParameters();
            param.Add("@Id", Id);

            param.Add("@output", dbType: DbType.String, direction: ParameterDirection.Output, size: int.MaxValue);//luôn để DataOutput trong stored procedure

            return await _sqlDataAccess.SaveDataUsingStoredProcedure<string>(proc, param);
        }

        #endregion

        #region DOCUMENT
        public async Task Document(string rabbitMqMessage)
        {
            try
            {
                var data = JsonConvert.DeserializeObject<DocumentDto>(rabbitMqMessage);
                Console.WriteLine("RabbitMQ Document Data: " + rabbitMqMessage);

                if (data != null)
                {
                    switch (data.RabbitMQType)
                    {
                        case CommonConst.RABBITMQ_TYPE_DELETE:
                            var deleteItem = await _esdDbContext.sysTbl_Document.FirstOrDefaultAsync(x => x.documentId == data.documentId);
                            data.row_version = deleteItem.row_version;
                            var result = await DeleteDocument(data);
                            Console.WriteLine("Delete Document Message: " + result);
                            break;

                        default:
                            break;
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

        public async Task<string> DeleteDocument(DocumentDto model)
        {
            string proc = "sysUsp_Document_Delete";
            var param = new DynamicParameters();
            param.Add("@documentId", model.documentId);
            param.Add("@row_version", model.row_version);
            param.Add("@output", dbType: DbType.String, direction: ParameterDirection.Output, size: int.MaxValue);

            return await _sqlDataAccess.SaveDataUsingStoredProcedure<int>(proc, param);
        }
        #endregion

        #region MACHINE
        public async Task ESDMachine(string rabbitMqMessage)
        {
            try
            {
                var data = JsonConvert.DeserializeObject<MachineDto>(rabbitMqMessage);
                Console.WriteLine("RabbitMQ Message: " + rabbitMqMessage);

                if (data != null)
                {
                    switch (data.RabbitMQType)
                    {
                        case CommonConst.RABBITMQ_TYPE_UPDATE:
                            await UpdateMachine(data);
                            break;

                        case CommonConst.RABBITMQ_TYPE_DELETE:
                            await DeleteReuseMachine(data);
                            break;

                        default:
                            _esdDbContext.ESDMachine.Add(AutoMapperConfig<MachineDto, ESDMachine>.Map(data));
                            await _esdDbContext.SaveChangesAsync();
                            break;
                    }

                    var record = await _esdDbContext.ESDMachine
                        .FirstOrDefaultAsync(x => x.MachineId == data.MachineId);

                    data.row_version = record.row_version;

                    switch (data.RabbitMQType)
                    {

                        case CommonConst.RABBITMQ_TYPE_UPDATE:
                        case CommonConst.RABBITMQ_TYPE_DELETE:
                            await _esMachineService.Update(data);
                            break;
                        default:
                            await _esMachineService.Insert(data);
                            break;
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

        private async Task UpdateMachine(MachineDto machineDto)
        {
            // string proc = "Usp_Machine_Modify";
            // var param = new DynamicParameters();
            // param.Add("@machineId", machineDto.MachineId);
            // param.Add("@machineCode", machineDto.MachineCode);
            // param.Add("@machineName", machineDto.MachineName);
            // param.Add("@isActived", machineDto.isActived);
            // param.Add("@modifiedBy", machineDto.modifiedBy);
            // param.Add("@row_version", machineDto.row_version);
            // param.Add("@output", dbType: DbType.String, direction: ParameterDirection.Output, size: int.MaxValue);
            // var returnMessage = await _sqlDataAccess.SaveDataUsingStoredProcedure<string>(proc, param);
            // Console.WriteLine("RabbitMQ Update Message: " + returnMessage);

            // return returnMessage;

            var recordToUpdate = await _esdDbContext.ESDMachine.FirstOrDefaultAsync(x => x.MachineId == machineDto.MachineId);

            recordToUpdate.MachineCode = machineDto.MachineCode;
            recordToUpdate.MachineName = machineDto.MachineName;
            recordToUpdate.isActived = machineDto.isActived;
            recordToUpdate.modifiedBy = machineDto.modifiedBy;
            recordToUpdate.modifiedDate = DateTime.Now;

            await _esdDbContext.SaveChangesAsync();
        }

        private async Task DeleteReuseMachine(MachineDto model)
        {
            var recordToUpdate = await _esdDbContext.ESDMachine
                        .FirstOrDefaultAsync(x => x.MachineId == model.MachineId);
            recordToUpdate.isActived = model.isActived;
            recordToUpdate.modifiedDate = model.modifiedDate;
            await _esdDbContext.SaveChangesAsync();
        }
        #endregion
    }
}
