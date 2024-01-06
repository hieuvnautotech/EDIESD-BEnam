using Microsoft.Extensions.Caching.Memory;
using ESD_EDI_BE.DbAccess;
using ESD_EDI_BE.Models.Dtos.Common;
using ESD_EDI_BE.Services.Cache;
using static ESD_EDI_BE.Extensions.ServiceExtensions;

namespace ESD_EDI_BE.Services.Common
{
    public interface IUserAuthorizationService
    {
        Task<IEnumerable<UserAuthorizationDto>> GetUserAuthorization();
        //void UpdateUserAuthorizationCache();
    }

    [SingletonRegistration]
    public class UserAuthorizationService : IUserAuthorizationService
    {
        private readonly ISqlDataAccess _sqlDataAccess;
        private readonly IMemoryCache _memoryCache;
        public UserAuthorizationService(ISqlDataAccess sqlDataAccess, IMemoryCache memoryCache)
        {
            _sqlDataAccess = sqlDataAccess;
            _memoryCache = memoryCache;
        }

        public async Task<IEnumerable<UserAuthorizationDto>> GetUserAuthorization()
        {
            string proc = "sysUsp_GetAuthorization";
            return await _sqlDataAccess.LoadDataUsingStoredProcedure<UserAuthorizationDto>(proc);
        }

        //public async void UpdateUserAuthorizationCache()
        //{
        //    var userAuthorization = await GetUserAuthorization();
        //    //_memoryCache.Remove("userAuthorization");
        //    //_memoryCache.Set("userAuthorization", userAuthorization);
        //    //await _sysCacheService.SetAllPermission(userAuthorization.ToList());
        //}
    }
}
