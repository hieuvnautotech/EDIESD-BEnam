using Dapper;
using ESD_EDI_BE.Extensions;
using ESD_EDI_BE.Models.Dtos;
using ESD_EDI_BE.Models.Dtos.Common;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;
using System.Data;
using ESD_EDI_BE.DbAccess;
using static ESD_EDI_BE.Extensions.ServiceExtensions;
using Newtonsoft.Json;
using ESD_EDI_BE.Models.Redis;

namespace ESD_EDI_BE.Services.EDI
{
    public interface IPolicyService
    {
        Task<ResponseModel<IEnumerable<PPORTAL_QUAL02_POLICY_Redis>?>> GetForCache();
        Task<ResponseModel<IEnumerable<PPORTAL_QUAL02_POLICYDto>?>> Get(PPORTAL_QUAL02_POLICYDto model);
        Task<ResponseModel<PPORTAL_QUAL02_POLICYDto>> GetById(long id);
        Task<ResponseModel<PPORTAL_QUAL02_POLICYDto?>> Create(PPORTAL_QUAL02_POLICYDto model);
        Task<ResponseModel<PPORTAL_QUAL02_POLICYDto?>> Modify(PPORTAL_QUAL02_POLICYDto model);
        Task<ResponseModel<PPORTAL_QUAL02_POLICYDto?>> Delete(PPORTAL_QUAL02_POLICYDto model);
    }
    // [ScopedRegistration]
    [SingletonRegistration]
    public class PolicyService : IPolicyService
    {
        private readonly ISqlDataAccess _sqlDataAccess;

        public PolicyService(ISqlDataAccess sqlDataAccess)
        {
            _sqlDataAccess = sqlDataAccess;
        }

        public async Task<ResponseModel<PPORTAL_QUAL02_POLICYDto?>> Create(PPORTAL_QUAL02_POLICYDto model)
        {
            var returnData = new ResponseModel<PPORTAL_QUAL02_POLICYDto?>();
            string proc = "Usp_Policy_Create";
            var param = new DynamicParameters();
            param.Add("@Id", model.Id);
            param.Add("@ITEM_CODE", model.ITEM_CODE);
            param.Add("@TRAND_TP", model.TRAND_TP);
            param.Add("@CTQ_NO", model.CTQ_NO);
            param.Add("@MIN_VALUE", model.MIN_VALUE);
            param.Add("@MAX_VALUE", model.MAX_VALUE);
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
                    var createdPolicy = await GetById(model.Id);
                    returnData.Data = createdPolicy.Data;
                    break;
                default:
                    returnData.HttpResponseCode = 400;
                    break;
            }
            return returnData;
        }

        public async Task<ResponseModel<IEnumerable<PPORTAL_QUAL02_POLICY_Redis>?>> GetForCache()
        {
            var returnData = new ResponseModel<IEnumerable<PPORTAL_QUAL02_POLICY_Redis>?>();
            var proc = $"Usp_Policy_GetForCache";

            var data = await _sqlDataAccess.LoadDataUsingStoredProcedure<PPORTAL_QUAL02_POLICY_Redis>(proc);

            if (!data.Any())
            {
                returnData.ResponseMessage = StaticReturnValue.NO_DATA;
                returnData.HttpResponseCode = 204;
            }
            else
            {
                returnData.Data = data;
            }

            return returnData;
        }

        public async Task<ResponseModel<PPORTAL_QUAL02_POLICYDto?>> Delete(PPORTAL_QUAL02_POLICYDto model)
        {
            string proc = "Usp_Policy_Delete";
            var param = new DynamicParameters();
            param.Add("@Id", model.Id);
            param.Add("@row_version", model.row_version);
            param.Add("@createdBy", model.createdBy);
            param.Add("@output", dbType: DbType.String, direction: ParameterDirection.Output, size: int.MaxValue);//luôn để DataOutput trong stored procedure

            var returnData = new ResponseModel<PPORTAL_QUAL02_POLICYDto?>();
            var result = await _sqlDataAccess.SaveDataUsingStoredProcedure<int>(proc, param);
            returnData.ResponseMessage = result;
            returnData.HttpResponseCode = result switch
            {
                StaticReturnValue.SYSTEM_ERROR => 500,
                StaticReturnValue.REFRESH_REQUIRED => 500,
                StaticReturnValue.SUCCESS => 200,
                _ => 400,
            };
            return returnData;
        }

        public async Task<ResponseModel<PPORTAL_QUAL02_POLICYDto?>> Modify(PPORTAL_QUAL02_POLICYDto model)
        {
            var returnData = new ResponseModel<PPORTAL_QUAL02_POLICYDto?>();
            string proc = "Usp_Policy_Modify";
            var param = new DynamicParameters();
            param.Add("@Id", model.Id);
            param.Add("@ITEM_CODE", model.ITEM_CODE);
            param.Add("@TRAND_TP", model.TRAND_TP);
            param.Add("@CTQ_NO", model.CTQ_NO);
            param.Add("@MIN_VALUE", model.MIN_VALUE);
            param.Add("@MAX_VALUE", model.MAX_VALUE);
            param.Add("@createdBy", model.createdBy);
            param.Add("@row_version", model.row_version);
            param.Add("@output", dbType: DbType.String, direction: ParameterDirection.Output, size: int.MaxValue);
            var result = await _sqlDataAccess.SaveDataUsingStoredProcedure<int>(proc, param);
            returnData.ResponseMessage = result;
            switch (result)
            {
                case StaticReturnValue.SYSTEM_ERROR:
                    returnData.HttpResponseCode = 500;
                    break;
                case StaticReturnValue.REFRESH_REQUIRED:
                    returnData.HttpResponseCode = 500;
                    break;
                case StaticReturnValue.SUCCESS:
                    returnData.HttpResponseCode = 200;
                    var modifiedPolicy = await GetById(model.Id);
                    returnData.Data = modifiedPolicy.Data;
                    break;
                default:
                    returnData.HttpResponseCode = 400;
                    break;
            }
            return returnData;
        }

        public async Task<ResponseModel<IEnumerable<PPORTAL_QUAL02_POLICYDto>?>> Get(PPORTAL_QUAL02_POLICYDto model)
        {
            var returnData = new ResponseModel<IEnumerable<PPORTAL_QUAL02_POLICYDto>?>();
            var proc = $"Usp_Policy_Get";
            var param = new DynamicParameters();
            param.Add("@page", model.page);
            param.Add("@pageSize", model.pageSize);
            param.Add("@ITEM_CODE", model.ITEM_CODE);
            param.Add("@TRAND_TP", model.TRAND_TP);
            param.Add("@CTQ_NO", model.CTQ_NO);
            param.Add("@isActived", model.@isActived);
            param.Add("@totalRow", 0, DbType.Int32, ParameterDirection.Output);
            var data = await _sqlDataAccess.LoadDataUsingStoredProcedure<PPORTAL_QUAL02_POLICYDto>(proc, param);

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

        public async Task<ResponseModel<PPORTAL_QUAL02_POLICYDto>> GetById(long id)
        {
            var returnData = new ResponseModel<PPORTAL_QUAL02_POLICYDto>();
            var proc = $"Usp_Policy_GetById";
            var param = new DynamicParameters();
            param.Add("@Id", id);

            var data = await _sqlDataAccess.LoadDataUsingStoredProcedure<PPORTAL_QUAL02_POLICYDto>(proc, param);
            returnData.Data = data.FirstOrDefault();
            if (!data.Any())
            {
                returnData.ResponseMessage = StaticReturnValue.NO_DATA;
                returnData.HttpResponseCode = 204;
            }
            return returnData;
        }
    }
}
