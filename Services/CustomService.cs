using Dapper;
using ESD_EDI_BE.DbAccess;
using ESD_EDI_BE.Extensions;
using ESD_EDI_BE.Models;
using ESD_EDI_BE.Models.Dtos;
using ESD_EDI_BE.Models.Dtos.Common;
using ESD_EDI_BE.Models.Validators;
using ESD_EDI_BE.Services.Base;
using System.Data;
using static ESD_EDI_BE.Extensions.ServiceExtensions;

namespace ESD_EDI_BE.Services
{
    public interface ICustomService
    {
        Task<ResponseModel<IEnumerable<T>>> GetForSelect<T>(string column, string table, string where, string order);
    }
    [ScopedRegistration]
    public class CustomService : ICustomService
    {
        private readonly ISqlDataAccess _sqlDataAccess;

        public CustomService(ISqlDataAccess sqlDataAccess)
        {
            _sqlDataAccess = sqlDataAccess;
        }

        public async Task<ResponseModel<IEnumerable<T>>> GetForSelect<T>(string column, string table, string where, string order)
        {
            var returnData = new ResponseModel<IEnumerable<T>>();
            var proc = $"sysUsp_Table_GetForSelect";
            var param = new DynamicParameters();
            param.Add("@column", column);
            param.Add("@table", table);
            param.Add("@where", where);
            param.Add("@order", order);

            var data = await _sqlDataAccess.LoadDataUsingStoredProcedure<T>(proc, param);

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

    }
}
