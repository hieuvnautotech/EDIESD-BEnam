using Dapper;
using ESD_EDI_BE.DbAccess;
using System.Data;
using static ESD_EDI_BE.Extensions.ServiceExtensions;

namespace ESD_EDI_BE.Services.Common
{
    public interface ILogoutService
    {
        Task<string> LogoutAsync(string token);
    }

    [ScopedRegistration]
    public class LogoutService : ILogoutService
    {
        private readonly ISqlDataAccess _sqlDataAccess;

        public LogoutService(ISqlDataAccess sqlDataAccess)
        {
            _sqlDataAccess = sqlDataAccess;
        }

        public async Task<string> LogoutAsync(string token)
        {
            string proc = "sysUsp_User_Logout";
            var param = new DynamicParameters();
            param.Add("@accessToken", token);
            param.Add("@output", dbType: DbType.String, direction: ParameterDirection.Output, size: int.MaxValue);//luôn để DataOutput trong stored procedure

            var data = await _sqlDataAccess.SaveDataUsingStoredProcedure<string>(proc, param);
            return data;
        }
    }
}
