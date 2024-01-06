using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using ESD_EDI_BE.CustomAttributes;
using ESD_EDI_BE.Extensions;
using ESD_EDI_BE.Models.Dtos.Common;
using ESD_EDI_BE.Services.Cache;
using ESD_EDI_BE.Services.Common;
using ESD_EDI_BE.Hubs;

namespace ESD_EDI_BE.Controllers.Standard.Configuration
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;
        // private readonly IMemoryCache _memoryCache;
        // private readonly IUserAuthorizationService _userAuthorizationService;
        private readonly IJwtService _jwtService;
        private readonly IMenuService _menuService;
        private readonly ISysCacheService _sysCacheService;
        private readonly SignalRHub _signalRHub;

        public RoleController(IMenuService menuService
            , IRoleService roleService
            // ,IMemoryCache memoryCache
            , IUserAuthorizationService userAuthorizationService
            , IJwtService jwtService
            , ISysCacheService sysCacheService
            , SignalRHub signalRHub
        )
        {
            _roleService = roleService;
            // _memoryCache = memoryCache;
            // _userAuthorizationService = userAuthorizationService;
            _jwtService = jwtService;
            _menuService = menuService;
            _sysCacheService = sysCacheService;
            _signalRHub = signalRHub;
        }

        [HttpGet]
        [PermissionAuthorization(PermissionConst.ROLE_READ)]
        public async Task<IActionResult> Get([FromQuery] PageModel pageInfo, string keyWord)
        {
            var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var roleName = _jwtService.GetRolenameFromToken(token);
            return Ok(await _roleService.Get(pageInfo, roleName, keyWord));
        }

        [HttpGet("get-menu-by-role/{roleId}")]
        [PermissionAuthorization(PermissionConst.ROLE_READ)]
        public async Task<IActionResult> GetMenuByRole(long roleId, [FromQuery] PageModel pageInfo, string keyWord)
        {
            var a = await _menuService.GetByRole(pageInfo, roleId, keyWord);
            return Ok(a);
        }

        [HttpPost("create-role")]
        [PermissionAuthorization(PermissionConst.ROLE_CREATE)]
        public async Task<IActionResult> Create(RoleDto model)
        {
            model.roleId = AutoId.AutoGenerate();
            var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            model.createdBy = _jwtService.GetUserIdFromToken(token);
            var result = await _roleService.Create(model);

            var returnData = new ResponseModel<RoleDto?>
            {
                ResponseMessage = result
            };
            switch (result)
            {
                case StaticReturnValue.SYSTEM_ERROR:
                    returnData.HttpResponseCode = 500;
                    break;
                case StaticReturnValue.SUCCESS:
                    returnData = await _roleService.GetById(model.roleId);
                    await _sysCacheService.SetRoleMenuPermissionsToRedis(returnData.Data.roleCode);
                    await _sysCacheService.SetRoleMenusToRedis(returnData.Data.roleCode);
                    await _sysCacheService.SetRoleMissingMenuPermissionsToRedis(returnData.Data.roleCode);
                    break;
                default:
                    returnData.HttpResponseCode = 400;
                    break;
            }

            return Ok(returnData);
        }

        [HttpPut("modify-role")]
        [PermissionAuthorization(PermissionConst.ROLE_UPDATE)]
        public async Task<IActionResult> Modify(RoleDto model)
        {
            var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            model.modifiedBy = _jwtService.GetUserIdFromToken(token);
            var result = await _roleService.Modify(model);

            var returnData = new ResponseModel<RoleDto?>
            {
                ResponseMessage = result
            };
            switch (result)
            {
                case StaticReturnValue.SYSTEM_ERROR:
                    returnData.HttpResponseCode = 500;
                    break;
                case StaticReturnValue.SUCCESS:
                    returnData = await _roleService.GetById(model.roleId);
                    break;
                default:
                    returnData.HttpResponseCode = 400;
                    break;
            }

            return Ok(returnData);
        }

        [HttpDelete("delete-role/{roleId}")]
        [PermissionAuthorization(PermissionConst.ROLE_DELETE)]
        public async Task<IActionResult> Delete(long roleId)
        {
            var roleDto = await _roleService.GetById(roleId);
            var roleCode = roleDto.Data.roleCode;
            var responseMessage = "model.role.error_message.essential_role_not_delete";

            if (string.IsNullOrEmpty(roleCode) || roleCode == "000" || roleCode == "001")
            {
                return Ok(responseMessage);
            }

            responseMessage = await _roleService.Delete(roleId);

            if (responseMessage == StaticReturnValue.SUCCESS)
            {
                var cacheKey_roleMenu = $"{CommonConst.CACHE_KEY_ROLE_MENU}_{roleCode}";
                var cacheKey_roleMissingPermission = $"{CommonConst.CACHE_KEY_ROLE_MISSING_MENU_PERMISSION}_{roleCode}";
                var cacheKey_rolePermission = $"{CommonConst.CACHE_KEY_ROLE_PERMISSIONS}_{roleCode}";
                string[] delKeys = { cacheKey_roleMenu, cacheKey_roleMissingPermission, cacheKey_rolePermission };
                await _sysCacheService.DelAsync(delKeys);
                await _signalRHub.SendRoleDelete(roleCode);
            }

            return Ok(responseMessage);
        }

        [HttpGet("get-all-menu")]
        public async Task<IActionResult> GetAllMenu()
        {
            var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var roleList = _jwtService.GetRolenameFromToken(token);
            return Ok(await _menuService.GetForSelect(roleList));
        }

        [HttpPost("set-permissions-for-role")]
        [PermissionAuthorization(PermissionConst.ROLE_UPDATE)]
        public async Task<IActionResult> SetPermissionForRole([FromBody] RoleDeleteDto model)
        {
            var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            model.createdBy = _jwtService.GetUserIdFromToken(token);

            var returnData = await _roleService.SetPermissionForRole(model);

            await _sysCacheService.SetRoleMenuPermissionsToRedis(model.roleCode);

            await _sysCacheService.SetRoleMissingMenuPermissionsToRedis(model.roleCode);

            await _signalRHub.SendUpdateRoleMissingPermissions(model.roleCode);

            return Ok(returnData);
        }

        [HttpPost("set-menu-for-role")]
        [PermissionAuthorization(PermissionConst.ROLE_UPDATE)]
        public async Task<IActionResult> SetMenuForRole([FromBody] RoleDto model)
        {
            var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            model.createdBy = _jwtService.GetUserIdFromToken(token);

            var data = await _roleService.SetMenuForRole(model);

            if (data.ResponseMessage == StaticReturnValue.SUCCESS)
            {
                await _sysCacheService.SetRoleMenusToRedis(model.roleCode);
                await _sysCacheService.SetRoleMenuPermissionsToRedis(model.roleCode);
                await _sysCacheService.SetRoleMissingMenuPermissionsToRedis(model.roleCode);

                await _signalRHub.SendRoleMenusUpdate(model.roleCode);
            }

            return Ok(data);
        }

        [HttpPost("delete-permission")]
        [PermissionAuthorization(PermissionConst.ROLE_DELETE)]
        public async Task<IActionResult> DeletePermission([FromBody] RoleDeleteDto model)
        {
            var data = await _roleService.DeletePermissionForRole(model);
            await _sysCacheService.SetRoleMenuPermissionsToRedis(model.roleCode);
            return Ok(data);
        }

        [HttpPost("delete-menu")]
        [PermissionAuthorization(PermissionConst.ROLE_DELETE)]
        public async Task<IActionResult> DeleteMenu([FromBody] RoleDeleteDto model)
        {
            var data = await _roleService.DeleteMenuForRole(model);

            await _sysCacheService.SetRoleMenusToRedis(model.roleCode);
            await _signalRHub.SendRoleMenusUpdate(model.roleCode);
            return Ok(data);
        }

        [HttpGet("get-missing-menu-permission")]
        public async Task<IActionResult> GetMissingMenuPermissions(string roleCode)
        {
            return Ok(await _roleService.GetMissingMenuPermissions(roleCode));
        }

        [HttpGet("get-role-permission")]
        public async Task<IActionResult> GetRoleMenu(string roleCode)
        {
            var data = await _sysCacheService.GetRoleMenuPermissionsFromRedis(roleCode);
            return Ok(data);
        }

        [HttpGet("get-menu-permission")]
        [PermissionAuthorization(PermissionConst.ROLE_SET_PERMISSION)]
        public async Task<IActionResult> GetMenuPermission(long menuId)
        {
            var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            var userRoleStr = _jwtService.GetRolenameFromToken(token);
            var userRole = userRoleStr.Replace(" ", "").Split(",");
            return Ok(await _menuService.GetMenuPermission(menuId, userRole ?? Array.Empty<string>()));
        }

        [HttpGet("get-role-menu-permission")]
        [PermissionAuthorization(PermissionConst.ROLE_SET_PERMISSION)]
        public async Task<IActionResult> GetRoleMenuPermission(long roleId, long menuId)
        {
            var data = await _roleService.GetMenuPermissionIdsByRoleId(roleId, menuId);
            return Ok(data);
        }
    }
}
