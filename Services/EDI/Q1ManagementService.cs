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
    public interface IQ1ManagementService
    {
        Task<ResponseModel<IEnumerable<PPORTAL_QUAL01_INFODto>?>> Get(PPORTAL_QUAL01_INFODto model);
        Task<ResponseModel<PPORTAL_QUAL01_INFODto?>> Create(PPORTAL_QUAL01_INFODto model);
        Task<ResponseModel<PPORTAL_QUAL01_INFODto?>> CreateByExcel(List<PPORTAL_QUAL01_INFODto> model);
        Task<ResponseModel<PPORTAL_QUAL01_INFODto?>> Modify(PPORTAL_QUAL01_INFODto model);
        Task<ResponseModel<PPORTAL_QUAL01_INFODto?>> Delete(long Id);
    }
    [ScopedRegistration]
    public class Q1ManagementService : IQ1ManagementService
    {
        private readonly ISqlDataAccess _sqlDataAccess;

        public Q1ManagementService(ISqlDataAccess sqlDataAccess)
        {
            _sqlDataAccess = sqlDataAccess;
        }

        public async Task<ResponseModel<PPORTAL_QUAL01_INFODto?>> Create(PPORTAL_QUAL01_INFODto model)
        {
            var returnData = new ResponseModel<PPORTAL_QUAL01_INFODto?>();
            string proc = "Usp_Q1Management_Create";
            var param = new DynamicParameters();
            param.Add("@ITEM_CODE", model.ITEM_CODE);
            param.Add("@INV_NO", model.INV_NO);
            param.Add("@INV_MAPPING_DTTM", model.INV_MAPPING_DTTM);
            param.Add("@BARCODE_NO", model.BARCODE_NO);
            param.Add("@S_BARCODE_NO", model.S_BARCODE_NO);
            param.Add("@LARGEBOX_QTY", model.LARGEBOX_QTY);
            param.Add("@SMALLBOX_QTY", model.SMALLBOX_QTY);
            param.Add("@QUAL_INFO1", model.QUAL_INFO1);
            param.Add("@QUAL_INFO2", model.QUAL_INFO2);
            param.Add("@QUAL_INFO3", model.QUAL_INFO3);
            param.Add("@QUAL_INFO4", model.QUAL_INFO4);
            param.Add("@QUAL_INFO5", model.QUAL_INFO5);
            param.Add("@QUAL_INFO6", model.QUAL_INFO6);
            param.Add("@QUAL_INFO7", model.QUAL_INFO7);
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
                    break;
                default:
                    returnData.HttpResponseCode = 400;
                    break;
            }
            return returnData;
        }

        public async Task<ResponseModel<PPORTAL_QUAL01_INFODto?>> CreateByExcel(List<PPORTAL_QUAL01_INFODto> model)
        {
            var returnData = new ResponseModel<PPORTAL_QUAL01_INFODto?>();

            var jsonLotList = JsonConvert.SerializeObject(model);

            string proc = "Usp_Q1Management_CreateByExcel";
            var param = new DynamicParameters();
            param.Add("@Jsonlist", jsonLotList);
            param.Add("@output", dbType: DbType.String, direction: ParameterDirection.Output, size: int.MaxValue);//luôn để DataOutput trong stored procedure

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
                    break;
                default:
                    returnData.HttpResponseCode = 400;
                    break;
            }
            return returnData;
        }

        public async Task<ResponseModel<IEnumerable<PPORTAL_QUAL01_INFODto>?>> Get(PPORTAL_QUAL01_INFODto model)
        {
            var returnData = new ResponseModel<IEnumerable<PPORTAL_QUAL01_INFODto>?>();
            var proc = $"Usp_Q1Management_Get";
            var param = new DynamicParameters();
            param.Add("@page", model.page);
            param.Add("@pageSize", model.pageSize);
            param.Add("@ITEM_CODE", model.ITEM_CODE);
            param.Add("@totalRow", 0, DbType.Int32, ParameterDirection.Output);
            var data = await _sqlDataAccess.LoadDataUsingStoredProcedure<PPORTAL_QUAL01_INFODto>(proc, param);

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
        public async Task<ResponseModel<PPORTAL_QUAL01_INFODto?>> Delete(long Id)
        {
            string proc = "Usp_Q1Management_Delete";
            var param = new DynamicParameters();
            param.Add("@Id", Id);
            param.Add("@output", dbType: DbType.String, direction: ParameterDirection.Output, size: int.MaxValue);//luôn để DataOutput trong stored procedure

            var returnData = new ResponseModel<PPORTAL_QUAL01_INFODto?>();
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
                    break;
                default:
                    returnData.HttpResponseCode = 400;
                    break;
            }

            return returnData;
        }
        public async Task<ResponseModel<PPORTAL_QUAL01_INFODto?>> Modify(PPORTAL_QUAL01_INFODto model)
        {
            var returnData = new ResponseModel<PPORTAL_QUAL01_INFODto?>();
            string proc = "Usp_Q1Management_Modify";
            var param = new DynamicParameters();
            param.Add("@Id", model.Id);
            param.Add("@ITEM_CODE", model.ITEM_CODE);
            param.Add("@INV_NO", model.INV_NO);
            param.Add("@INV_MAPPING_DTTM", model.INV_MAPPING_DTTM);
            param.Add("@BARCODE_NO", model.BARCODE_NO);
            param.Add("@S_BARCODE_NO", model.S_BARCODE_NO);
            param.Add("@LARGEBOX_QTY", model.LARGEBOX_QTY);
            param.Add("@SMALLBOX_QTY", model.SMALLBOX_QTY);
            param.Add("@QUAL_INFO1", model.QUAL_INFO1);
            param.Add("@QUAL_INFO2", model.QUAL_INFO2);
            param.Add("@QUAL_INFO3", model.QUAL_INFO3);
            param.Add("@QUAL_INFO4", model.QUAL_INFO4);
            param.Add("@QUAL_INFO5", model.QUAL_INFO5);
            param.Add("@QUAL_INFO6", model.QUAL_INFO6);
            param.Add("@QUAL_INFO7", model.QUAL_INFO7);
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
                    break;
                default:
                    returnData.HttpResponseCode = 400;
                    break;
            }
            return returnData;
        }
    }
}
