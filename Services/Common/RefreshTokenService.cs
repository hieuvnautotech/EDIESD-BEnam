using Dapper;
using Microsoft.Extensions.Caching.Memory;
using ESD_EDI_BE.DbAccess;
using ESD_EDI_BE.Models.Dtos.Common;
using System.Data;
using static ESD_EDI_BE.Extensions.ServiceExtensions;

namespace ESD_EDI_BE.Services.Common
{
    public interface IRefreshTokenService
    {
        Task<string> Create(RefreshTokenDto model);
        Task<string> Modify(RefreshTokenDto model);
        Task<IEnumerable<string>> GetUserTokensByUserId(long userId);
        Task<RefreshTokenDto?> Get(UserRefreshTokenRequest request);
        Task<HashSet<string>> GetAvailables();

        // void UpdateAvailableTokensCache();

    }

    [SingletonRegistration]
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly ISqlDataAccess _sqlDataAccess;
        // private readonly IMemoryCache _memoryCache;

        public RefreshTokenService(
            ISqlDataAccess sqlDataAccess
            // , IMemoryCache memoryCache
            )
        {
            _sqlDataAccess = sqlDataAccess;
            // _memoryCache = memoryCache;
        }

        public async Task<string> Create(RefreshTokenDto model)
        {
            string proc = "sysUsp_RefreshToken_Create";
            var param = new DynamicParameters();
            param.Add("@accessToken", model.accessToken);
            param.Add("@refreshToken", model.refreshToken);
            param.Add("@createdDate", model.createdDate);
            param.Add("@expirationDate", model.expiredDate);
            param.Add("@ipAddress", model.ipAddress);
            param.Add("@userId", model.userId);
            param.Add("@isOnApp", model.isOnApp);
            param.Add("@output", dbType: DbType.String, direction: ParameterDirection.Output, size: int.MaxValue);//luôn để DataOutput trong stored procedure
            return await _sqlDataAccess.SaveDataUsingStoredProcedure<string>(proc, param); ;
        }

        public async Task<RefreshTokenDto?> Get(UserRefreshTokenRequest request)
        {
            string proc = "sysUsp_RefreshToken_Get";
            var param = new DynamicParameters();
            param.Add("@accessToken", request.expiredToken);
            param.Add("@refreshToken", request.refreshToken);
            param.Add("@ipAddress", request.ipAddress);

            var data = await _sqlDataAccess.LoadDataUsingStoredProcedure<RefreshTokenDto>(proc, param);
            return data.FirstOrDefault() ?? default;
        }

        public async Task<HashSet<string>> GetAvailables()
        {
            string proc = "sysUsp_RefreshToken_GetAvailables";
            var param = new DynamicParameters();
            var data = await _sqlDataAccess.LoadDataUsingStoredProcedure<string>(proc, param);
            return data.ToHashSet();
        }

        public async Task<IEnumerable<string>> GetUserTokensByUserId(long userId)
        {
            string proc = "sysUsp_RefreshToken_GetUserTokensByUserId";
            var param = new DynamicParameters();
            param.Add("@userId", userId);
            var returnData = await _sqlDataAccess.LoadDataUsingStoredProcedure<string>(proc, param);
            return returnData;
        }

        public async Task<string> Modify(RefreshTokenDto model)
        {
            string proc = "sysUsp_RefreshToken_Modify";
            var param = new DynamicParameters();
            param.Add("@refreshTokenId", model.refreshTokenId);
            param.Add("@output", dbType: DbType.String, direction: ParameterDirection.Output);//luôn để DataOutput trong stored procedure
            return await _sqlDataAccess.SaveDataUsingStoredProcedure<string>(proc, param);
        }

        // public async void UpdateAvailableTokensCache()
        // {
        //     var availableTokens = await GetAvailables();
        //     _memoryCache.Remove("availableTokens");
        //     _memoryCache.Set("availableTokens", availableTokens);
        // }
    }
}
