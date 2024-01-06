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
    public interface IQ2ManagementService
    {
        Task<ResponseModel<IEnumerable<PPORTAL_QUAL02_INFODto>?>> Get(int page, int pageSize, string ITEM_CODE, string TRAND_TP, DateTime? StartDate, DateTime? EndDate);

    }
    [ScopedRegistration]
    public class Q2ManagementService : IQ2ManagementService
    {
        private readonly ISqlDataAccess _sqlDataAccess;

        public Q2ManagementService(ISqlDataAccess sqlDataAccess)
        {
            _sqlDataAccess = sqlDataAccess;
        }


        public async Task<ResponseModel<IEnumerable<PPORTAL_QUAL02_INFODto>?>> Get(int page, int pageSize, string ITEM_CODE, string TRAND_TP, DateTime? StartDate, DateTime? EndDate)
        {
            var returnData = new ResponseModel<IEnumerable<PPORTAL_QUAL02_INFODto>?>();
            var proc = $"Usp_Q2Management_Get";
            var param = new DynamicParameters();
            param.Add("@page", page);
            param.Add("@pageSize", pageSize);
            param.Add("@ITEM_CODE", ITEM_CODE);
            param.Add("@TRAND_TP", TRAND_TP);
            param.Add("@StartDate", StartDate);
            param.Add("@EndDate", EndDate);
            param.Add("@totalRow", 0, DbType.Int32, ParameterDirection.Output);
            var data = await _sqlDataAccess.LoadDataUsingStoredProcedure<PPORTAL_QUAL02_INFODto>(proc, param);

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

    }
}
