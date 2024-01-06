using Dapper;
using ESD_EDI_BE.DbAccess;
using ESD_EDI_BE.Extensions;
using ESD_EDI_BE.Models;
using ESD_EDI_BE.Models.Dtos.Common;
using ESD_EDI_BE.Services.Base;
using System.Data;
using static ESD_EDI_BE.Extensions.ServiceExtensions;

namespace ESD_EDI_BE.Services.Common
{
    public interface ICommonDetailService
    {
        Task<ResponseModel<IEnumerable<CommonDetailDto>?>> GetByCommonMasterCode(PageModel pageInfo, long userId, string commonMasterCode, bool? isActived, string keyWord);
        Task<ResponseModel<IEnumerable<CommonDetailDto>?>> GetForSelectByMasterCode(string masterCode);
        Task<ResponseModel<IEnumerable<CommonDetailDto>?>> GetPermissionType();
        Task<string> Create(CommonDetailDto model);
        Task<ResponseModel<CommonDetailDto?>> GetById(long id);
        Task<string> Modify(CommonDetailDto model);
        Task<string> DeleteReuse(CommonDetailDto model);
        Task<ResponseModel<IEnumerable<CommonDetailDto>?>> GetUnCreatedPermissionType();

    }
    [ScopedRegistration]
    public class CommonDetailService : ICommonDetailService
    {
        private readonly ISqlDataAccess _sqlDataAccess;

        public CommonDetailService(ISqlDataAccess sqlDataAccess)
        {
            _sqlDataAccess = sqlDataAccess;
        }

        public async Task<ResponseModel<IEnumerable<CommonDetailDto>?>> GetByCommonMasterCode(PageModel pageInfo, long userId, string commonMasterCode, bool? isActived, string keyWord)
        {
            try
            {
                var returnData = new ResponseModel<IEnumerable<CommonDetailDto>?>();
                string proc = "sysUsp_CommonDetail_GetByCommonMasterCode";
                var param = new DynamicParameters();
                param.Add("@commonMasterCode", commonMasterCode);
                param.Add("@isActived", isActived);
                param.Add("@keyWord", keyWord);
                param.Add("@page", pageInfo.page);
                param.Add("@pageSize", pageInfo.pageSize);
                param.Add("@totalRow", 0, DbType.Int32, ParameterDirection.Output);

                var data = await _sqlDataAccess.LoadDataUsingStoredProcedure<CommonDetailDto>(proc, param);
                returnData.Data = data;
                returnData.TotalRow = param.Get<int>("totalRow");
                if (!data.Any())
                {
                    returnData.HttpResponseCode = 204;
                    returnData.ResponseMessage = StaticReturnValue.NO_DATA;
                }
                return returnData;
            }
            catch (Exception e)
            {

                throw;
            }

        }

        public async Task<string> Create(CommonDetailDto model)
        {
            string proc = "sysUsp_CommonDetail_Create";
            var param = new DynamicParameters();
            param.Add("@commonMasterCode", model.commonMasterCode);
            param.Add("@commonDetailId", model.commonDetailId);
            param.Add("@commonDetailName", model.commonDetailName?.ToUpper());
            param.Add("@createdBy", model.createdBy);
            param.Add("@output", dbType: DbType.String, direction: ParameterDirection.Output, size: int.MaxValue);//luôn để DataOutput trong stored procedure

            return await _sqlDataAccess.SaveDataUsingStoredProcedure<string>(proc, param);
        }

        public async Task<ResponseModel<CommonDetailDto?>> GetById(long id)
        {
            var returnData = new ResponseModel<CommonDetailDto?>();
            string proc = "sysUsp_CommonDetail_GetById";
            var param = new DynamicParameters();
            param.Add("@commonDetailId", id);

            var data = await _sqlDataAccess.LoadDataUsingStoredProcedure<CommonDetailDto?>(proc, param);
            returnData.Data = data.FirstOrDefault();
            if (!data.Any())
            {
                returnData.HttpResponseCode = 204;
                returnData.ResponseMessage = StaticReturnValue.NO_DATA;
            }
            return returnData;
        }

        public async Task<string> Modify(CommonDetailDto model)
        {
            string proc = "sysUsp_CommonDetail_Modify";
            var param = new DynamicParameters();
            param.Add("@commonDetailId", model.commonDetailId);
            param.Add("@commonDetailName", model.commonDetailName?.ToUpper());
            param.Add("@modifiedBy", model.modifiedBy);
            param.Add("@row_version", model.row_version);
            param.Add("@output", dbType: DbType.String, direction: ParameterDirection.Output, size: int.MaxValue);//luôn để DataOutput trong stored procedure

            return await _sqlDataAccess.SaveDataUsingStoredProcedure<string>(proc, param);
        }

        public async Task<ResponseModel<IEnumerable<CommonDetailDto>?>> GetPermissionType()
        {
            try
            {
                var returnData = new ResponseModel<IEnumerable<CommonDetailDto>?>();
                string proc = "sysUsp_CommonDetail_GetPermissionType";
                var data = await _sqlDataAccess.LoadDataUsingStoredProcedure<CommonDetailDto>(proc);
                if (!data.Any())
                {

                    returnData.HttpResponseCode = 204;
                    returnData.ResponseMessage = StaticReturnValue.NO_DATA;
                }
                else
                {
                    returnData.Data = data;
                }
                return returnData;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<string> DeleteReuse(CommonDetailDto model)
        {
            string proc = "sysUsp_CommonDetail_DeleteReuse";
            var param = new DynamicParameters();
            param.Add("@commonDetailId", model.commonDetailId);
            param.Add("@isActived", model.isActived);
            param.Add("@modifiedBy", model.modifiedBy);
            param.Add("@row_version", model.row_version);
            param.Add("@output", dbType: DbType.String, direction: ParameterDirection.Output, size: int.MaxValue);//luôn để DataOutput trong stored procedure

            return await _sqlDataAccess.SaveDataUsingStoredProcedure<int>(proc, param);
        }

        public async Task<ResponseModel<IEnumerable<CommonDetailDto>?>> GetUnCreatedPermissionType()
        {
            try
            {
                var returnData = new ResponseModel<IEnumerable<CommonDetailDto>?>();
                string proc = "sysUsp_CommonDetail_GetUnCreatedPermissionType";
                var data = await _sqlDataAccess.LoadDataUsingStoredProcedure<CommonDetailDto>(proc);
                if (!data.Any())
                {

                    returnData.HttpResponseCode = 204;
                    returnData.ResponseMessage = StaticReturnValue.NO_DATA;
                }
                else
                {
                    returnData.Data = data;
                }
                return returnData;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<ResponseModel<IEnumerable<CommonDetailDto>?>> GetForSelectByMasterCode(string masterCode)
        {
            var returnData = new ResponseModel<IEnumerable<CommonDetailDto>?>();
            string proc = "sysUsp_CommonDetail_GetForSelectByMasterCode";
            var param = new DynamicParameters();
            param.Add("@commonMasterCode", masterCode);
            var data = await _sqlDataAccess.LoadDataUsingStoredProcedure<CommonDetailDto>(proc, param);
            if (!data.Any())
            {

                returnData.HttpResponseCode = 204;
                returnData.ResponseMessage = StaticReturnValue.NO_DATA;
            }
            else
            {
                returnData.Data = data;
            }
            return returnData;
        }
    }
}
