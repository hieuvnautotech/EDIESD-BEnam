using Dapper;
using ESD_EDI_BE.DbAccess;
using ESD_EDI_BE.Extensions;
using ESD_EDI_BE.Helpers;
using ESD_EDI_BE.Models;
using ESD_EDI_BE.Models.Dtos;
using ESD_EDI_BE.Models.Dtos.Common;
using System.Data;
using System.Numerics;
using static ESD_EDI_BE.Extensions.ServiceExtensions;

namespace ESD_EDI_BE.Services.Common
{
    public interface IVersionAppService
    {
        Task<ResponseModel<IEnumerable<VersionAppDto>>> GetAll();
        Task<string> Modify(VersionAppDto model);
        Task<ResponseModel<VersionAppDto>> GetAppByCode(string app_code);
    }

    [SingletonRegistration]
    public class VersionAppService : IVersionAppService
    {
        private readonly ISqlDataAccess _sqlDataAccess;

        public VersionAppService(ISqlDataAccess sqlDataAccess)
        {
            _sqlDataAccess = sqlDataAccess;
        }


        public async Task<ResponseModel<IEnumerable<VersionAppDto>>> GetAll()
        {
            var returnData = new ResponseModel<IEnumerable<VersionAppDto>>();
            var proc = $"sysUsp_VersionApp_GetAll";
            //var param = new DynamicParameters();
            var data = await _sqlDataAccess.LoadDataUsingStoredProcedure<VersionAppDto>(proc);

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

        public async Task<ResponseModel<VersionAppDto>> GetAppByCode(string app_code)
        {
            var returnData = new ResponseModel<VersionAppDto>();
            var proc = $"sysUsp_VersionApp_GetAppByCode";
            var param = new DynamicParameters();
            param.Add("@app_code", app_code);
            var data = await _sqlDataAccess.LoadDataUsingStoredProcedure<VersionAppDto>(proc, param);

            if (!data.Any())
            {
                returnData.HttpResponseCode = 204;
                returnData.ResponseMessage = StaticReturnValue.NO_DATA;
            }
            else
            {
                returnData.Data = data.FirstOrDefault();
            }
            return returnData;
        }

        public async Task<string> Modify(VersionAppDto model)
        {
            string proc = "sysUsp_VersionApp_Modify";
            var param = new DynamicParameters();
            param.Add("@id_app", model.id_app);
            param.Add("@app_version", model.app_version);
            param.Add("@CHPlay_version", model.CHPlay_version);
            param.Add("@link_url", model.link_url);
            param.Add("@name_file", model.file != null ? model.name_file : null);
            param.Add("@createdBy", model.createdBy);
            param.Add("@output", dbType: DbType.String, direction: ParameterDirection.Output, size: int.MaxValue);//luôn để DataOutput trong stored procedure

            return await _sqlDataAccess.SaveDataUsingStoredProcedure<string>(proc, param);
        }
    }
}
