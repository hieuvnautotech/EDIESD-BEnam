using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using ESD_EDI_BE.Extensions;
using ESD_EDI_BE.Models.Dtos.Common;
using ESD_EDI_BE.Services.Cache;
using ESD_EDI_BE.Services.Common;

namespace ESD_EDI_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogoutController : ControllerBase
    {
        private readonly ILogoutService _logoutService;
        private readonly IMemoryCache _memoryCache;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly ISysCacheService _sysCacheService;

        public LogoutController(ILogoutService logoutService,
            IMemoryCache memoryCache,
            IRefreshTokenService refreshTokenService,
            IUserAuthorizationService userAuthorizationService,
            ISysCacheService sysCacheService
            )
        {
            _logoutService = logoutService;
            _memoryCache = memoryCache;
            _refreshTokenService = refreshTokenService;
            _sysCacheService = sysCacheService;
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            var returnData = new ResponseModel<bool>();
            var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (token != "logout_token")
            {

                var result = await _logoutService.LogoutAsync(token);
                returnData.ResponseMessage = result;
                switch (result)
                {
                    case StaticReturnValue.SUCCESS:
                        //var availableTokens = _refreshTokenService.GetAvailables();
                        //_memoryCache.Remove("availableTokens");
                        //_memoryCache.Set("availableTokens", availableTokens.Result);

                        // _refreshTokenService.UpdateAvailableTokensCache();
                        await _sysCacheService.RemoveAvailableTokenFromRedis(token);

                        //_memoryCache.Remove("userAuthorization");
                        //_memoryCache.Set("userAuthorization", userAuthorization.Result);

                        // await _sysCacheService.UpdateUserAuthorizationCache();

                        returnData.Data = true;
                        break;
                    default:
                        returnData.HttpResponseCode = 500;
                        returnData.Data = false;
                        break;
                }
            }
            else
            {
                returnData.ResponseMessage = StaticReturnValue.SUCCESS;
                returnData.Data = true;
            }

            return Ok(returnData);
        }
    }
}
