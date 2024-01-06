using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using ESD_EDI_BE.CustomAttributes;
using ESD_EDI_BE.Extensions;
using ESD_EDI_BE.Helpers;
using ESD_EDI_BE.Models.Dtos.Common;
using ESD_EDI_BE.Services.Common;
using System.IdentityModel.Tokens.Jwt;
using ESD_EDI_BE.Services.Cache;

namespace ESD_EDI_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RefreshTokenController : ControllerBase
    {
        private readonly IJwtService _jwtService;
        private readonly IUserService _userService;
        private readonly IRefreshTokenService _refreshTokenService;
        // private readonly IMemoryCache _memoryCache;
        private readonly ISysCacheService _sysCacheService;

        public RefreshTokenController(
            IJwtService jwtService
            , IUserService userService
            , IRefreshTokenService refreshTokenService
            // , IMemoryCache memoryCache
            , ISysCacheService sysCacheService
        )
        {
            _jwtService = jwtService;
            _userService = userService;
            _refreshTokenService = refreshTokenService;
            // _memoryCache = memoryCache;
            _sysCacheService = sysCacheService;
        }


        // This method refreshes an expired JWT token by generating a new access and refresh token for the user.
        // It takes in a UserRefreshTokenRequest object from the HTTP request body.
        [HttpPost]
        [AllowAll]
        public async Task<IActionResult> RefreshToken([FromBody] UserRefreshTokenRequest request)
        {
            var returnData = new ResponseModel<AuthorizationResponse>();
            if (!ModelState.IsValid)
            {
                // If the request is not valid, populate the response object with an error message
                // and return a 400 HTTP response code
                returnData.HttpResponseCode = 400;
                returnData.ResponseMessage = StaticReturnValue.OBJECT_INVALID;
                return Ok(returnData);
            }

            // Attempt to get the JWT token from the expired token provided in the request body
            var token = GetJwtToken(request.expiredToken);

            // Retrieve the remote IP address of the user who made the request and set it in the request object
            request.ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            request.ipAddress = UserIPHelper.UserIp;

            // Retrieve the refresh token associated with the user from the _refreshTokenService
            var refreshToken = await _refreshTokenService.Get(request);

            // Validate the JWT token against the refresh token using the ValidateToken method
            var authorizationResponse = ValidateToken(token, refreshToken);
            if (!authorizationResponse.isSuccess)
            {
                // If the validation fails, return an error message with a 400 HTTP response code
                returnData.HttpResponseCode = 400;
                returnData.ResponseMessage = authorizationResponse.reason;
                return Ok(returnData);
            }

            // Retrieve the user ID from the JWT token and attempt to retrieve the user associated with that ID from the _userService
            var userId = token.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;
            var user = await _userService.GetByUserId(long.Parse(userId));
            if (user == null)
            {
                // If the user is not found, return an error message with a 403 HTTP response code
                returnData.HttpResponseCode = 403;
                returnData.ResponseMessage = StaticReturnValue.USER_NOTFOUND;
                return Ok(returnData);
            }

            // Generate a new access token and refresh token for the user using the _jwtService
            var authResponse = await _jwtService.GetTokenAsync(user.Data);

            // Create a new refresh token DTO with the new access and refresh tokens, user ID, IP address, and other metadata
            RefreshTokenDto refreshTokenDto = new()
            {
                accessToken = authResponse.accessToken,
                refreshToken = authResponse.refreshToken,
                createdDate = DateTime.UtcNow,
                expiredDate = refreshToken.expiredDate,
                //IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                //IpAddress = HttpContext.Connection.LocalIpAddress?.ToString(),
                //IpAddress = HttpContext.Request.Headers["X-Forwarded-For"].ToString(),
                ipAddress = UserIPHelper.UserIp,
                isValidated = false,
                userId = user.Data.userId,
            };

            // Store the new refresh token in the _refreshTokenService
            var result = await _refreshTokenService.Create(refreshTokenDto);
            if (result == StaticReturnValue.SUCCESS)
            {
                // If the storage of the new refresh token is successful, retrieve the list of available refresh tokens
                // from the _refreshTokenService and cache it in the memory cache. Populate the response object with the new
                // access and refresh tokens and a success message, and return a 200 HTTP response code.
                //var availableTokens = _refreshTokenService.GetAvailables();
                //_memoryCache.Remove("availableTokens");
                //_memoryCache.Set("availableTokens", availableTokens.Result);

                // _refreshTokenService.UpdateAvailableTokensCache();
                await _sysCacheService.SetAvailableTokensToRedis();

                returnData.Data = authResponse;
                returnData.ResponseMessage = StaticReturnValue.SUCCESS;
                return Ok(returnData);
            }
            else
            {
                returnData.HttpResponseCode = 500;
                returnData.ResponseMessage = StaticReturnValue.SYSTEM_ERROR;
                return Ok(returnData);
            }
        }

        private static JwtSecurityToken GetJwtToken(string expiredToken)
        {
            JwtSecurityTokenHandler jwtSecurityTokenHandler = new();
            return jwtSecurityTokenHandler.ReadJwtToken(expiredToken);
        }

        private static AuthorizationResponse ValidateToken(JwtSecurityToken token, RefreshTokenDto refreshToken)
        {
            if (refreshToken == null)
            {
                return new AuthorizationResponse { isSuccess = false, reason = StaticReturnValue.INVALID_REFRESH_TOKEN };
            }

            if (refreshToken.isActive == false)
            {
                return new AuthorizationResponse { isSuccess = false, reason = StaticReturnValue.REFRESH_TOKEN_EXPIRED };
            }

            if (token.ValidTo > DateTime.UtcNow)
            {
                return new AuthorizationResponse { isSuccess = false, reason = StaticReturnValue.TOKEN_IS_NOT_EXPIRED };
            }

            return new AuthorizationResponse();
        }
    }
}
