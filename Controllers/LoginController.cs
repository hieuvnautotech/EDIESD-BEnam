using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Net.Http.Headers;
using ESD_EDI_BE.Extensions;
using ESD_EDI_BE.Helpers;
using ESD_EDI_BE.Hubs;
using ESD_EDI_BE.Models.Dtos.Common;
using ESD_EDI_BE.Services.Cache;
using ESD_EDI_BE.Services.Common;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace ESD_EDI_BE.Controllers
{
    [Route("api/[controller]")]
    [Produces("application/json")]
    [ApiController]
    [SwaggerTag("Login")]
    public class LoginController : ControllerBase
    {

        private readonly ILoginService _loginService;
        private readonly IUserService _userService;
        private readonly IJwtService _jwtService;
        private readonly IRefreshTokenService _refreshTokenService;
        // private readonly IMemoryCache _memoryCache;
        // private readonly IHttpContextAccessor _httpContextAccessor;
        // private readonly IUserAuthorizationService _userAuthorizationService;
        private readonly ISysCacheService _sysCacheService;
        private readonly SignalRHub _signalRHub;
        public LoginController(
            ILoginService loginService
            , IUserService userService
            , IJwtService jwtService
            , IRefreshTokenService refreshTokenService
            // , IMemoryCache memoryCache
            // , IHttpContextAccessor httpContextAccessor
            // , IUserAuthorizationService userAuthorizationService
            , ISysCacheService sysCacheService
            , SignalRHub signalRHub
        //, IMenuService menuService
        )
        {
            _loginService = loginService;
            _userService = userService;
            _jwtService = jwtService;
            _refreshTokenService = refreshTokenService;
            // _memoryCache = memoryCache;
            // _httpContextAccessor = httpContextAccessor;
            // _userAuthorizationService = userAuthorizationService;
            _sysCacheService = sysCacheService;
            _signalRHub = signalRHub;
        }

        [HttpPost("check-login")]
        //[SwaggerOperation(Tags = new[] { "Login" })]
        [SwaggerOperation
        (
            Summary = "Check userid/password",
            Description = "Returns access token."
        )]

        public async Task<IActionResult> Login([FromBody] LoginModelDto loginModel)
        {
            // Check Login Condition
            var result = await _loginService.CheckLogin(loginModel);
            var returnData = new ResponseModel<AuthorizationResponse>
            {
                ResponseMessage = result
            };

            switch (result)
            {
                //Login successfully
                case StaticReturnValue.SUCCESS:

                    //Get User Information
                    var data = await _userService.GetByUserName(loginModel.userName);

                    // Generate Access Token and Refresh Token
                    var authResponse = await _jwtService.GetTokenAsync(data.Data);
                    data.Data.Token = authResponse.accessToken;

                    var userRefreshTokenModel = new RefreshTokenDto
                    {
                        accessToken = authResponse.accessToken,
                        refreshToken = authResponse.refreshToken,
                        createdDate = DateTime.UtcNow,
                        expiredDate = DateTime.UtcNow.AddDays(60),
                        ipAddress = UserIPHelper.UserIp,
                        isValidated = false,
                        userId = data.Data.userId,
                        isOnApp = loginModel.isOnApp
                    };

                    // Insert Access Token and Refresh Token into Database
                    if (await _refreshTokenService.Create(userRefreshTokenModel) == StaticReturnValue.SUCCESS)
                    {
                        // Update Available Access Token to Redis
                        await _sysCacheService.SetAvailableTokensToRedis();

                        // Update User Authorization to Redis
                        // await _sysCacheService.UpdateUserAuthorizationCache();

                        data.Data.RefreshToken = authResponse.refreshToken;

                        // Update User last login time to Database
                        await SetLastLogin(data.Data, loginModel.isOnApp);
                        returnData.Data = authResponse;
                    }
                    else
                    {
                        SetInternalServerError(returnData);
                    }

                    break;

                case StaticReturnValue.ACCOUNT_PASSWORD_INVALID:
                case StaticReturnValue.ACCOUNT_BLOCKED:
                    SetBadRequest(returnData);
                    break;

                default:
                    SetInternalServerError(returnData);
                    break;
            }

            return Ok(returnData);
        }

        private async Task SetLastLogin(UserDto userDto, bool? isOnApp)
        {
            if (isOnApp == null || isOnApp == false)
            {
                userDto.lastLoginOnWeb = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
            else
            {
                userDto.lastLoginOnApp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
            userDto.isOnline = true;
            // await _sysCacheService.SetOnlineUserToRedis(userDto);

            if (isOnApp == null || isOnApp == false)
            {
                await _userService.SetLastLoginOnWeb(userDto);
            }
            else
            {
                await _userService.SetLastLoginOnApp(userDto);
            }

            await _signalRHub.SetUserLoggedIn(userDto);
        }

        //private void UpdateAvailableTokensCache()
        //{
        //    var availableTokens = _refreshTokenService.GetAvailables();
        //    _memoryCache.Remove("availableTokens");
        //    _memoryCache.Set("availableTokens", availableTokens.Result);
        //}

        //private void UpdateUserAuthorizationCache()
        //{
        //    var userAuthorization = _userAuthorizationService.GetUserAuthorization();
        //    _memoryCache.Remove("userAuthorization");
        //    _memoryCache.Set("userAuthorization", userAuthorization.Result);
        //}

        private static void SetBadRequest(ResponseModel<AuthorizationResponse> returnData)
        {
            returnData.HttpResponseCode = 400;
        }

        private static void SetInternalServerError(ResponseModel<AuthorizationResponse> returnData)
        {
            returnData.HttpResponseCode = 500;
            returnData.ResponseMessage = StaticReturnValue.SYSTEM_ERROR;
        }


        [HttpGet("user-info")]
        [SwaggerOperation
        (
            Summary = "Get user info by ID",
            Description = "Returns user info and user menus."
        )]
        public async Task<IActionResult> GetUserInfo()
        {
            var userAgent = Request.Headers[HeaderNames.UserAgent].ToString();
            bool? isAndroid = null;
            if (userAgent.Contains("android") || userAgent.Contains("iphone") || userAgent.Contains("ipad") || userAgent.Contains("ipod") || userAgent.Contains("blackberry") || userAgent.Contains("windows phone"))
            {
                isAndroid = true;
            }
            else
            {
                isAndroid = false;
            }
            var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            //long userId = _jwtService.GetUserIdFromToken(token);
            var userId = _jwtService.GetUserIdFromToken(token);
            var user = await _userService.GetUserInfo(userId);
            var returnData = new ResponseModel<UserDto>();

            if (user != null)
            {
                returnData.Data = user;
                returnData.ResponseMessage = StaticReturnValue.SUCCESS;
            }
            else
            {
                returnData.HttpResponseCode = 400;
                returnData.ResponseMessage = StaticReturnValue.NO_DATA;
            }
            return Ok(returnData);
        }

    }
}
