using Dapper;
using ESD_EDI_BE.Extensions;
using ESD_EDI_BE.Models.Dtos;
using ESD_EDI_BE.Models.Dtos.Common;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;
using System.Data;
using ESD_EDI_BE.DbAccess;
using static ESD_EDI_BE.Extensions.ServiceExtensions;
using Newtonsoft.Json;

namespace ESD_EDI_BE.Services.EDI
{
    public interface IMachineService
    {
        // Task<ResponseModel<IEnumerable<MachineDto>?>> Get();
        Task<ResponseModel<IEnumerable<MachineDto>?>> Get(MachineDto model);
        Task<ResponseModel<MachineDto>> GetById(long machineId);
        Task<ResponseModel<MachineDto>> GetByCode(string machineCode);
        Task<ResponseModel<MachineDto?>> Create(MachineDto model);
        Task<ResponseModel<MachineDto?>> Modify(MachineDto model);
        Task<ResponseModel<string>> Delete(MachineDto model);

    }
    [ScopedRegistration]
    public class MachineService : IMachineService
    {
        private readonly ISqlDataAccess _sqlDataAccess;

        public MachineService(ISqlDataAccess sqlDataAccess)
        {
            _sqlDataAccess = sqlDataAccess;
        }

        public async Task<ResponseModel<IEnumerable<MachineDto>?>> Get(MachineDto model)
        {
            var returnData = new ResponseModel<IEnumerable<MachineDto>?>();
            var proc = $"Usp_Machine_Get";
            var param = new DynamicParameters();
            param.Add("@page", model.page);
            param.Add("@pageSize", model.pageSize);
            param.Add("@keyword", model.MachineCode);
            param.Add("@isActived", model.@isActived);
            param.Add("@totalRow", 0, DbType.Int32, ParameterDirection.Output);
            var data = await _sqlDataAccess.LoadDataUsingStoredProcedure<MachineDto>(proc, param);

            if (!data.Any())
            {
                returnData.ResponseMessage = StaticReturnValue.NO_DATA;
                returnData.HttpResponseCode = 204;
            }
            else
            {
                returnData.Data = data;
                returnData.TotalRow = param.Get<int>("totalRow");
            }

            return returnData;
        }

        public async Task<ResponseModel<MachineDto?>> Create(MachineDto model)
        {
            var returnData = new ResponseModel<MachineDto?>();
            string proc = "Usp_Machine_Create";
            var param = new DynamicParameters();
            param.Add("@MachineId", model.MachineId);
            param.Add("@MachineCode", model.MachineCode);
            param.Add("@MachineName", model.MachineName);
            param.Add("@createdBy", model.createdBy);
            param.Add("@output", dbType: DbType.String, direction: ParameterDirection.Output, size: int.MaxValue);
            var result = await _sqlDataAccess.SaveDataUsingStoredProcedure<int>(proc, param);
            returnData.ResponseMessage = result;
            switch (result)
            {
                case StaticReturnValue.SYSTEM_ERROR:
                    returnData.HttpResponseCode = 500;
                    break;
                case StaticReturnValue.SUCCESS:
                    returnData.HttpResponseCode = 200;
                    returnData.ResponseMessage = result;
                    var data = await GetById(model.MachineId);
                    returnData.Data = data.Data;
                    break;
                default:
                    returnData.HttpResponseCode = 400;
                    break;
            }
            return returnData;
        }

        public async Task<ResponseModel<string>> Delete(MachineDto model)
        {
            string proc = "Usp_Machine_Delete";
            var param = new DynamicParameters();
            param.Add("@machineId", model.MachineId);
            param.Add("@row_version", model.row_version);
            param.Add("@modifiedBy", model.modifiedBy);
            param.Add("@output", dbType: DbType.String, direction: ParameterDirection.Output, size: int.MaxValue);//luôn để DataOutput trong stored procedure

            var result = await _sqlDataAccess.SaveDataUsingStoredProcedure<string>(proc, param);
            var returnData = new ResponseModel<string>
            {
                ResponseMessage = result,
                HttpResponseCode = result switch
                {
                    StaticReturnValue.SYSTEM_ERROR => 500,
                    StaticReturnValue.REFRESH_REQUIRED => 500,
                    StaticReturnValue.SUCCESS => 200,
                    _ => 400,
                }
            };
            return returnData;
        }

        public async Task<ResponseModel<MachineDto?>> Modify(MachineDto model)
        {
            var returnData = new ResponseModel<MachineDto?>();
            string proc = "Usp_Machine_Modify";
            var param = new DynamicParameters();
            param.Add("@machineId", model.MachineId);
            param.Add("@machineCode", model.MachineCode);
            param.Add("@machineName", model.MachineName);
            param.Add("@isActived", model.isActived);
            param.Add("@modifiedBy", model.modifiedBy);
            param.Add("@row_version", model.row_version);
            param.Add("@output", dbType: DbType.String, direction: ParameterDirection.Output, size: int.MaxValue);
            var result = await _sqlDataAccess.SaveDataUsingStoredProcedure<int>(proc, param);
            returnData.ResponseMessage = result;
            // returnData.HttpResponseCode = result switch
            // {
            //     StaticReturnValue.SYSTEM_ERROR => 500,
            //     StaticReturnValue.REFRESH_REQUIRED => 500,
            //     StaticReturnValue.SUCCESS => 200,
            //     _ => 400,
            // };

            switch (result)
            {
                case StaticReturnValue.SYSTEM_ERROR:
                case StaticReturnValue.REFRESH_REQUIRED:
                    returnData.HttpResponseCode = 500;
                    break;
                case StaticReturnValue.SUCCESS:
                    returnData.HttpResponseCode = 200;
                    var data = await GetById(model.MachineId);
                    returnData.Data = data.Data;
                    break;
                default:
                    returnData.HttpResponseCode = 400;
                    break;
            }

            return returnData;
        }

        public async Task<ResponseModel<MachineDto>> GetByCode(string machineCode)
        {
            var returnData = new ResponseModel<MachineDto>();
            var proc = $"Usp_Machine_GetByCode";
            var param = new DynamicParameters();
            param.Add("@MachineCode", machineCode);

            var data = await _sqlDataAccess.LoadDataUsingStoredProcedure<MachineDto>(proc, param);

            if (!data.Any())
            {
                returnData.ResponseMessage = StaticReturnValue.NO_DATA;
                returnData.HttpResponseCode = 204;
            }
            else
            {
                returnData.Data = data.FirstOrDefault();
            }

            return returnData;
        }

        public async Task<ResponseModel<MachineDto>> GetById(long machineId)
        {
            var returnData = new ResponseModel<MachineDto>();
            var proc = $"Usp_Machine_GetById";
            var param = new DynamicParameters();
            param.Add("@machineId", machineId);

            var data = await _sqlDataAccess.LoadDataUsingStoredProcedure<MachineDto>(proc, param);

            if (!data.Any())
            {
                returnData.ResponseMessage = StaticReturnValue.NO_DATA;
                returnData.HttpResponseCode = 204;
            }
            else
            {
                returnData.Data = data.FirstOrDefault();
            }

            return returnData;
        }
    }
}
