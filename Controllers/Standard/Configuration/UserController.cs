using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using ESD_EDI_BE.CustomAttributes;
using ESD_EDI_BE.Extensions;
using ESD_EDI_BE.Models.Dtos;
using ESD_EDI_BE.Models.Dtos.Common;
using ESD_EDI_BE.Models.Validators;
using ESD_EDI_BE.Services.Cache;
using ESD_EDI_BE.Services.Common;
using Swashbuckle.AspNetCore.Annotations;
using ESD_EDI_BE.Hubs;

namespace ESD_EDI_BE.Controllers.Standard.Configuration
{
    [Route("api/[controller]")]
    [ApiController]
    [SwaggerTag("User")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        // private readonly IMemoryCache _memoryCache;
        private readonly IUserAuthorizationService _userAuthorizationService;
        private readonly IJwtService _jwtService;
        private readonly IRoleService _roleService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly ISysCacheService _sysCacheService;
        private readonly SignalRHub _signalRHub;
        public UserController(
            IUserService userService
            // ,IMemoryCache memoryCache
            , IUserAuthorizationService userAuthorizationService
            , IJwtService jwtService
            , IRoleService roleService
            , IRefreshTokenService refreshTokenService
            , ISysCacheService sysCacheService
            , SignalRHub signalRHub
            )
        {
            _userService = userService;
            // _memoryCache = memoryCache;
            _userAuthorizationService = userAuthorizationService;
            _jwtService = jwtService;
            _roleService = roleService;
            _refreshTokenService = refreshTokenService;
            _sysCacheService = sysCacheService;
            _signalRHub = signalRHub;
        }

        [HttpGet]
        [PermissionAuthorization(PermissionConst.USER_READ)]
        public async Task<IActionResult> Get([FromQuery] PageModel pageInfo, string? keyword, bool showDelete = true)
        {
            var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            // var userId = _jwtService.ValidateToken(token);
            var userId = await _jwtService.ValidateTokenAsync(token);
            var userRole = await _userService.GetUserRole(long.Parse(userId));
            if (userRole.Contains(RoleConst.ROOT))
            {
                return Ok(await _userService.Get(pageInfo, keyword, showDelete));
            }
            else
            {
                return Ok(await _userService.GetExceptRoot(pageInfo, keyword, showDelete));
            }
        }


        [HttpGet("get-userrole")]
        [PermissionAuthorization(PermissionConst.USER_READ)]
        public async Task<IActionResult> GetUserRole(long userId)
        {
            var returnData = new ResponseModel<IEnumerable<string>?>
            {
                Data = await _userService.GetUserRole(userId)
            };

            if (!returnData.Data.Any())
            {
                returnData.HttpResponseCode = 400;
                returnData.ResponseMessage = StaticReturnValue.NO_DATA;
            }

            return Ok(returnData);
        }

        [HttpGet("get-userpermission")]
        [PermissionAuthorization(PermissionConst.USER_READ)]
        public async Task<IActionResult> GetUserPermission(long userId)
        {
            var returnData = new ResponseModel<IEnumerable<string>?>
            {
                Data = await _userService.GetUserPermission(userId)
            };
            if (!returnData.Data.Any())
            {
                returnData.HttpResponseCode = 400;
                returnData.ResponseMessage = StaticReturnValue.NO_DATA;
            }

            return Ok(returnData);
        }

        [HttpGet("get-user-missing-permission/{userId}")]
        [AllowAll]
        public async Task<IActionResult> GetUserMissingPermission(long userId)
        {
            var returnData = new ResponseModel<IEnumerable<string>?>
            {
                Data = await _userService.GetUserMissingPermission(userId)
            };
            if (!returnData.Data.Any())
            {
                returnData.HttpResponseCode = 400;
                returnData.ResponseMessage = StaticReturnValue.NO_DATA;
            }

            return Ok(returnData);
        }

        [HttpPost("create-user")]
        [PermissionAuthorization(PermissionConst.USER_CREATE)]
        public async Task<IActionResult> Create(UserDto userInfo)
        {
            userInfo.userId = AutoId.AutoGenerate();
            var result = await _userService.Create(userInfo);

            var returnData = new ResponseModel<UserDto?>
            {
                ResponseMessage = result
            };
            switch (result)
            {
                case StaticReturnValue.SYSTEM_ERROR:
                    returnData.HttpResponseCode = 500;
                    break;
                case StaticReturnValue.SUCCESS:
                    if (userInfo.Roles != null)
                        await _userService.SetUserInfoRole(userInfo);
                    returnData = await _userService.GetByUserId(userInfo.userId);
                    returnData.ResponseMessage = StaticReturnValue.SUCCESS;
                    break;
                default:
                    returnData.HttpResponseCode = 400;
                    break;
            }

            return Ok(returnData);
        }

        [HttpPut("change-userpassword")]
        [AllowAll]
        public async Task<IActionResult> ChangeUserPassword(UserDto userInfo)
        {
            var returnData = new ResponseModel<UserDto?>();
            var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var userName = _jwtService.GetUserNameFromToken(token);
            if (userName != userInfo.userName)
            {
                returnData.HttpResponseCode = 400;
                returnData.ResponseMessage = StaticReturnValue.CHANGE_PASSWORD_NOT_ALLOWED;
            }

            else
            {
                var result = await _userService.ChangeUserPassword(userInfo);
                returnData.ResponseMessage = result;
                switch (result)
                {
                    case StaticReturnValue.SYSTEM_ERROR:
                        returnData.HttpResponseCode = 500;
                        break;
                    case StaticReturnValue.SUCCESS:
                        break;
                    default:
                        returnData.HttpResponseCode = 400;
                        break;
                }
            }

            return Ok(returnData);
        }

        [HttpPut("change-userpassword-by-root")]
        [PermissionAuthorization(PermissionConst.USER_UPDATE)]
        public async Task<IActionResult> ChangeUserPasswordByRoot(UserDto userInfo)
        {
            var returnData = new ResponseModel<UserDto?>();
            var result = await _userService.ChangeUserPasswordByRoot(userInfo);
            returnData.ResponseMessage = result;
            switch (result)
            {
                case StaticReturnValue.SYSTEM_ERROR:
                    returnData.HttpResponseCode = 500;
                    break;
                case StaticReturnValue.SUCCESS:
                    break;
                default:
                    returnData.HttpResponseCode = 400;
                    break;
            }

            return Ok(returnData);
        }

        [HttpPut("set-role-for-user")]
        [PermissionAuthorization(PermissionConst.USER_UPDATE)]
        public async Task<IActionResult> SetRoleForUser(UserDto userInfo)
        {
            var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            // userInfo.modifiedBy = _jwtService.GetUserIdFromToken(token);
            userInfo.modifiedBy = _jwtService.GetUserIdFromToken(token);
            var result = await _userService.SetUserInfoRole(userInfo);

            var returnData = new ResponseModel<UserDto?>
            {
                ResponseMessage = result
            };
            switch (result)
            {
                case StaticReturnValue.SYSTEM_ERROR:
                    returnData.HttpResponseCode = 500;
                    break;
                case StaticReturnValue.SUCCESS:
                    // var userAuthorization = await _userAuthorizationService.GetUserAuthorization();
                    //_memoryCache.Remove("userAuthorization");
                    //_memoryCache.Set("userAuthorization", userAuthorization.Result);
                    // await _sysCacheService.SetAllPermission(userAuthorization.ToList());
                    break;
                default:
                    returnData.HttpResponseCode = 400;
                    break;
            }

            return Ok(returnData);
        }

        [HttpGet("check-loggedin")]
        [AllowAll]
        public async Task<IActionResult> CheckLoggedinUser()
        {
            var returnData = new ResponseModel<bool>();
            var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            // var userId = _jwtService.GetUserIdFromToken(token);
            var userId = _jwtService.GetUserIdFromToken(token);
            var check = false;
            if (userId > 0)
            {
                var user = await _userService.GetByUserId(userId);
                if (user.Data != null)
                {
                    check = user.Data.isActived ?? false;
                }

            }

            if (!check)
                returnData.HttpResponseCode = 401;

            returnData.Data = check;
            return Ok(returnData);
        }

        [HttpGet("get-role/{UserId}")]
        [PermissionAuthorization(PermissionConst.USER_READ)]
        public async Task<IActionResult> GetRoleByUser(long UserId)
        {
            return Ok(await _userService.GetRoleByUser(UserId));
        }

        [HttpDelete("delete-user")]
        [PermissionAuthorization(PermissionConst.USER_DELETE)]
        public async Task<IActionResult> Delete([FromBody] UserDto model)
        {
            var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var userLogin = await _jwtService.ValidateTokenAsync(token);
            model.createdBy = long.Parse(userLogin);

            var result = await _userService.Delete(model);

            var returnData = new ResponseModel<UserDto?>
            {
                ResponseMessage = result
            };
            switch (result)
            {
                case StaticReturnValue.SYSTEM_ERROR:
                    returnData.HttpResponseCode = 500;
                    break;
                case StaticReturnValue.SUCCESS:
                    break;
                default:
                    returnData.HttpResponseCode = 400;
                    break;
            }

            return Ok(returnData);
        }

        [HttpGet("get-all-role")]
        [AllowAll]
        public async Task<IActionResult> GetAllRole()
        {
            var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            // var userId = _jwtService.ValidateToken(token);
            var userId = await _jwtService.ValidateTokenAsync(token);
            var userRole = await _userService.GetUserRole(long.Parse(userId));
            return Ok(await _roleService.GetForSelect(userRole.ToList()));
        }

        //[HttpGet("get-staffs")]
        //[AllowAll]
        //public async Task<IActionResult> GetStaffs(long? userId = null)
        //{
        //    var returnData = await _userService.GetStaffs(userId);
        //    return Ok(returnData);
        //}

        [HttpPut("modify-user")]
        [PermissionAuthorization(PermissionConst.USER_UPDATE)]
        public async Task<IActionResult> Modify([FromBody] UserDto model)
        {
            var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            // model.modifiedBy = _jwtService.GetUserIdFromToken(token);
            model.modifiedBy = _jwtService.GetUserIdFromToken(token);
            var result = await _userService.Modify(model);

            var returnData = new ResponseModel<UserDto?>
            {
                ResponseMessage = result
            };
            switch (result)
            {
                case StaticReturnValue.SYSTEM_ERROR:
                    returnData.HttpResponseCode = 500;
                    break;
                case StaticReturnValue.SUCCESS:
                    // var userAuthorization = await _userAuthorizationService.GetUserAuthorization();
                    //_memoryCache.Remove("userAuthorization");
                    //_memoryCache.Set("userAuthorization", userAuthorization.Result);
                    // await _sysCacheService.SetAllPermission(userAuthorization.ToList());

                    var userToken = await _refreshTokenService.GetUserTokensByUserId(model.userId);
                    await _signalRHub.SendUserRoleUpdate(userToken.FirstOrDefault(), model.userId.ToString());
                    var updatedUser = await _userService.GetByUserId(model.userId);
                    returnData.Data = updatedUser.Data;

                    break;
                default:
                    returnData.HttpResponseCode = 400;
                    break;
            }

            return Ok(returnData);

        }

        [HttpGet("get-user/{UserId}")]
        [PermissionAuthorization(PermissionConst.USER_READ)]
        public async Task<IActionResult> GetUserById(long UserId)
        {
            return Ok(await _userService.GetByUserId(UserId));
        }

        [HttpGet("get-user-menu")]
        [SwaggerOperation
        (
            Summary = "Get user menu list by useId",
            Description = "Returns user menu list."
        )]
        public async Task<IActionResult> GetUserMenu()
        {
            var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            // long userId = _jwtService.GetUserIdFromToken(token);
            long userId = _jwtService.GetUserIdFromToken(token);
            var menu = await _userService.GetUserMenu(userId);
            var returnData = new ResponseModel<IEnumerable<MenuDto>>();

            if (menu != null)
            {
                returnData.Data = menu;
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
