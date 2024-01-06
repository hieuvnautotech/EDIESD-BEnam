using Dapper;
using Microsoft.Extensions.Caching.Memory;
using ESD_EDI_BE.DbAccess;
using ESD_EDI_BE.Extensions;
using ESD_EDI_BE.Helpers;
using ESD_EDI_BE.Models;
using ESD_EDI_BE.Models.Dtos.Common;
using ESD_EDI_BE.Services.Base;
using ESD_EDI_BE.Services.Cache;
using System.Data;
using System.Data.SqlClient;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;
using static ESD_EDI_BE.Extensions.ServiceExtensions;
using ESD_EDI_BE.Models.Validators;

namespace ESD_EDI_BE.Services.Common
{
    public interface IRoleService
    {
        Task<ResponseModel<IEnumerable<RoleDto>?>> Get(PageModel pageInfo, string roleName, string keyWord);
        Task<ResponseModel<RoleDto?>> GetById(long roleId);
        Task<string> Create(RoleDto model);
        Task<string> Modify(RoleDto model);
        Task<string> Delete(long id);
        //Task<string> SetPermission(RoleDto model);
        Task<ResponseModel<IEnumerable<RoleDto>?>> GetForSelect(List<string> roles);
        Task<ResponseModel<string>> SetPermissionForRole(RoleDeleteDto model);
        Task<ResponseModel<string>> SetMenuForRole(RoleDto model);
        Task<ResponseModel<string>> DeletePermissionForRole(RoleDeleteDto model);
        Task<ResponseModel<string>> DeleteMenuForRole(RoleDeleteDto model);
        Task<IEnumerable<long>> GetMenuPermissionIdsByRoleId(long roleId, long menuId);
        Task<IEnumerable<RoleDto>> GetMenusByRoleCode(string? roleCode);
        Task<IEnumerable<RoleDto>> GetMissingMenuPermissions(string? roleCode = null);
        Task<IEnumerable<Role_Permission>> GetMenuPermissions(string? roleCode = null);
    }

    [SingletonRegistration]
    public class RoleService : IRoleService
    {
        private readonly ISqlDataAccess _sqlDataAccess;

        public RoleService(ISqlDataAccess sqlDataAccess)
        {
            _sqlDataAccess = sqlDataAccess;
        }

        public async Task<string> Create(RoleDto model)
        {
            // if (!ValidateModel.CheckValid(model, new sysTbl_Role()))
            // {
            //     return StaticReturnValue.OBJECT_INVALID; //Object Invalid
            // }

            var validator = new RoleValidator();
            var validateResults = validator.Validate(model);

            if (!validateResults.IsValid)
            {
                return validateResults.Errors[0].ToString();
            }

            string proc = "sysUsp_Role_Create";
            var param = new DynamicParameters();
            param.Add("@roleId", model.roleId);
            param.Add("@roleName", model.roleName?.Trim().ToUpper());
            param.Add("@roleDescription", model.roleDescription?.Trim());
            param.Add("@createdBy", model.createdBy);
            param.Add("@output", dbType: DbType.String, direction: ParameterDirection.Output, size: int.MaxValue);//luôn để DataOutput trong stored procedure

            return await _sqlDataAccess.SaveDataUsingStoredProcedure<string>(proc, param);
        }

        public async Task<string> Delete(long id)
        {
            string proc = "sysUsp_Role_Delete";
            var param = new DynamicParameters();
            param.Add("@roleId", id);
            param.Add("@output", dbType: DbType.String, direction: ParameterDirection.Output, size: int.MaxValue);//luôn để DataOutput trong stored procedure

            return await _sqlDataAccess.SaveDataUsingStoredProcedure<int>(proc, param);
        }

        public async Task<ResponseModel<IEnumerable<RoleDto>?>> Get(PageModel pageInfo, string roleName, string keyWord)
        {
            try
            {
                var returnData = new ResponseModel<IEnumerable<RoleDto>?>();
                var roleList = roleName.Split(",");
                string proc = "sysUsp_Role_Get";

                var param = new DynamicParameters();
                if (roleList.Contains(RoleConst.ROOT))
                {
                    param.Add("@roleRoot", "000");
                }
                else
                {
                    param.Add("@roleRoot", null);
                }
                param.Add("@keyword", keyWord);
                param.Add("@page", pageInfo.page);
                param.Add("@pageSize", pageInfo.pageSize);
                param.Add("@totalRow", 0, DbType.Int32, ParameterDirection.Output);

                var data = await _sqlDataAccess.LoadDataUsingStoredProcedure<RoleDto>(proc, param);
                returnData.Data = data;
                returnData.TotalRow = param.Get<int>("totalRow");
                if (!data.Any())
                {
                    returnData.HttpResponseCode = 204;
                    returnData.ResponseMessage = StaticReturnValue.NO_DATA;
                }
                return returnData;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ResponseModel<RoleDto?>> GetById(long roleId)
        {
            var returnData = new ResponseModel<RoleDto?>();
            string proc = "sysUsp_Role_GetById";
            var param = new DynamicParameters();
            param.Add("@roleId", roleId);

            var data = await _sqlDataAccess.LoadDataUsingStoredProcedure<RoleDto?>(proc, param);
            returnData.Data = data.FirstOrDefault();
            returnData.ResponseMessage = StaticReturnValue.SUCCESS;
            if (!data.Any())
            {
                returnData.HttpResponseCode = 204;
                returnData.ResponseMessage = StaticReturnValue.NO_DATA;
            }
            return returnData;
        }

        public async Task<string> Modify(RoleDto model)
        {

            if (!ValidateModel.CheckValid(model, new sysTbl_Role()))
            {
                return StaticReturnValue.OBJECT_INVALID; //Object Invalid
            }

            string proc = "sysUsp_Role_Modify";
            var param = new DynamicParameters();
            param.Add("@roleId", model.roleId);
            param.Add("@roleName", model.roleName?.Trim().ToUpper());
            param.Add("@roleDescription", model.roleDescription?.Trim());
            param.Add("@modifiedBy", model.modifiedBy);
            param.Add("@row_version", model.row_version);
            param.Add("@output", dbType: DbType.String, direction: ParameterDirection.Output, size: int.MaxValue);//luôn để DataOutput trong stored procedure

            return await _sqlDataAccess.SaveDataUsingStoredProcedure<string>(proc, param);
        }

        public async Task<ResponseModel<IEnumerable<RoleDto>?>> GetForSelect(List<string> roles)
        {
            var returnData = new ResponseModel<IEnumerable<RoleDto>?>();
            var proc = $"sysUsp_Role_GetForSelect";
            var param = new DynamicParameters();
            if (roles.Contains(RoleConst.ROOT))
                param.Add("@isRoot", true);
            else
                param.Add("@isRoot", false);

            var data = await _sqlDataAccess.LoadDataUsingStoredProcedure<RoleDto>(proc, param);
            returnData.Data = data;
            if (!data.Any())
            {
                returnData.ResponseMessage = StaticReturnValue.NO_DATA;
                returnData.HttpResponseCode = 204;
            }

            return returnData;
        }

        public async Task<ResponseModel<string>> SetPermissionForRole(RoleDeleteDto model)
        {
            try
            {
                var returnData = new ResponseModel<string>();
                if (model.menuPermissionIds == null)
                {
                    returnData.HttpResponseCode = 204;
                    returnData.ResponseMessage = StaticReturnValue.OBJECT_INVALID;
                    return returnData;
                }

                // var current_MenuPermissionIs = await GetMenuPermissionIdsByRoleId(model.roleId, model.menuId);

                // var delete_MenuPermissionIds = current_MenuPermissionIs.Except(model.menuPermissionIds);
                // var add_MenuPermissionIds = model.menuPermissionIds.Except(current_MenuPermissionIs);

                //string proc = "sysUsp_Role_AddPermission";
                string proc = "sysUsp_Role_SetMenuPermission";
                var param = new DynamicParameters();
                param.Add("@roleId", model.roleId);
                param.Add("@menuId", model.menuId);
                param.Add("@createdBy", model.createdBy);
                param.Add("@menuPermissionIds", ParameterTvp.GetTableValuedParameter_BigInt(model.menuPermissionIds));
                // param.Add("@delete_MenuPermissionIds", ParameterTvp.GetTableValuedParameter_BigInt(delete_MenuPermissionIds));
                // param.Add("@add_MenuPermissionIds", ParameterTvp.GetTableValuedParameter_BigInt(add_MenuPermissionIds));
                param.Add("@output", dbType: DbType.String, direction: ParameterDirection.Output, size: int.MaxValue);

                var data = await _sqlDataAccess.SaveDataUsingStoredProcedure<string>(proc, param);
                returnData.ResponseMessage = data;
                return returnData;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ResponseModel<string>> SetMenuForRole(RoleDto model)
        {
            try
            {
                var returnData = new ResponseModel<string>();
                if (!model.MenuIds.Any())
                {
                    returnData.HttpResponseCode = 204;
                    returnData.ResponseMessage = StaticReturnValue.OBJECT_INVALID;
                    return returnData;
                }
                var current_MenuIs = await GetMenuIdsByRoleId(model.roleId);


                var delete_MenuIds = current_MenuIs.Except(model.MenuIds);
                var add_MenuIds = model.MenuIds.Except(current_MenuIs);

                string proc = "sysUsp_Role_SetMenu";
                var param = new DynamicParameters();
                param.Add("@roleId", model.roleId);

                param.Add("@delete_MenuIds", ParameterTvp.GetTableValuedParameter_BigInt(delete_MenuIds));
                param.Add("@add_MenuIds", ParameterTvp.GetTableValuedParameter_BigInt(add_MenuIds));

                param.Add("@menuIds", ParameterTvp.GetTableValuedParameter_BigInt(model.MenuIds));


                param.Add("@createdBy", model.createdBy);
                param.Add("@output", dbType: DbType.String, direction: ParameterDirection.Output, size: int.MaxValue);

                var data = await _sqlDataAccess.SaveDataUsingStoredProcedure<string>(proc, param);
                returnData.ResponseMessage = data;
                return returnData;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<ResponseModel<string>> DeletePermissionForRole(RoleDeleteDto model)
        {
            try
            {
                var returnData = new ResponseModel<string>();
                if (!model.menuPermissionIds.Any())
                {
                    returnData.HttpResponseCode = 204;
                    returnData.ResponseMessage = StaticReturnValue.OBJECT_INVALID;
                    return returnData;
                }

                string proc = "sysUsp_Role_DeletePermission";
                var param = new DynamicParameters();
                param.Add("@roleId", model.roleId);
                param.Add("@menuPermissionIds", ParameterTvp.GetTableValuedParameter_BigInt(model.menuPermissionIds));
                param.Add("@output", dbType: DbType.String, direction: ParameterDirection.Output, size: int.MaxValue);

                var data = await _sqlDataAccess.SaveDataUsingStoredProcedure<string>(proc, param);
                returnData.ResponseMessage = data;

                //var userAuthorization = _userAuthorizationService.GetUserAuthorization();
                //_memoryCache.Remove("userAuthorization");
                //_memoryCache.Set("userAuthorization", userAuthorization.Result);

                // await _sysCacheService.UpdateUserAuthorizationCache();

                return returnData;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<ResponseModel<string>> DeleteMenuForRole(RoleDeleteDto model)
        {
            try
            {
                var returnData = new ResponseModel<string>();

                string proc = "sysUsp_Role_DeleteMenu";
                var param = new DynamicParameters();
                param.Add("@roleId", model.roleId);
                param.Add("@menuId", model.menuId);
                param.Add("@output", dbType: DbType.String, direction: ParameterDirection.Output, size: int.MaxValue);

                var data = await _sqlDataAccess.SaveDataUsingStoredProcedure<string>(proc, param);
                returnData.ResponseMessage = data;
                return returnData;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<IEnumerable<long>> GetMenuPermissionIdsByRoleId(long roleId, long menuId)
        {
            string query = @"SELECT
                                trmp.menu_PermissionId
                            FROM dbo.sysTbl_Role_Menu_Permission trmp
                            JOIN dbo.sysTbl_Menu_Permission tmp
                              ON tmp.Id = trmp.menu_PermissionId
                            WHERE trmp.roleId = @roleId
                            AND tmp.menuId = @menuId;";
            var param = new DynamicParameters();
            param.Add("@roleId", roleId);
            param.Add("@menuId", menuId);

            var data = await _sqlDataAccess.LoadDataUsingRawQuery<long>(query, param);
            return data;
        }

        public async Task<IEnumerable<long>> GetMenuIdsByRoleId(long roleId)
        {
            string query = @"SELECT m.menuId
                            FROM dbo.sysTbl_Menu m
                            WHERE (m.menuId IN (SELECT menuId FROM sysTbl_Role_Menu  WHERE roleId = @roleId ))";
            var param = new DynamicParameters();
            param.Add("@roleId", roleId);
            var data = await _sqlDataAccess.LoadDataUsingRawQuery<long>(query, param);
            return data;
        }

        public async Task<IEnumerable<RoleDto>> GetMenusByRoleCode(string? roleCode = null)
        {
            var returnData = new List<RoleDto>();
            string proc = "sysUsp_Role_GetMenus";
            var param = new DynamicParameters();
            param.Add("@roleCode", roleCode);

            var data = await _sqlDataAccess.LoadDataUsingStoredProcedure<MenuDto>(proc, param);

            if (data.Any())
            {
                returnData = data
                    .GroupBy(p => new { p.roleId, p.roleCode })
                    .Select(g => new RoleDto
                    {
                        roleId = (long)g.Key.roleId,
                        roleCode = g.Key.roleCode,
                        Menus = g.Select(p => new MenuDto()
                        {
                            menuId = p.menuId,
                            parentId = p.parentId,
                            parentMenuName = p.parentMenuName,
                            ParentMenuLanguageKey = p.ParentMenuLanguageKey,
                            menuName = p.menuName,
                            menuIcon = p.menuIcon,
                            languageKey = p.languageKey,
                            menuComponent = p.menuComponent,
                            navigateUrl = p.navigateUrl,
                            forRoot = p.forRoot,
                            forApp = p.forApp,
                            menuLevel = p.menuLevel,
                            sortOrder = p.sortOrder,
                            roleId = p.roleId,
                            roleCode = p.roleCode,
                        })
                    })
                    .ToList();
            }

            return returnData;
        }

        public async Task<IEnumerable<RoleDto>> GetMissingMenuPermissions(string? roleCode = null)
        {
            var returnData = new List<RoleDto>();
            string proc = "sysUsp_Role_GetMissingMenuPermissions";
            var param = new DynamicParameters();
            param.Add("@roleCode", roleCode);

            var data = await _sqlDataAccess.LoadDataUsingStoredProcedure<RoleMissingMenuPermission>(proc, param);
            if (data.Any())
            {
                returnData = data
                    .GroupBy(p => new { p.roleCode })
                    .Select(g => new RoleDto
                    {
                        roleCode = g.Key.roleCode,
                        MissingMenuPermissions = g.Select(p => new RoleMissingMenuPermission()
                        {
                            Id = p.Id,
                            MP_Description = p.MP_Description,
                            roleCode = p.roleCode,

                        })
                    })
                    .ToList();
            }

            return returnData;
        }

        public async Task<IEnumerable<Role_Permission>> GetMenuPermissions(string? roleCode = null)
        {
            var returnData = new List<Role_Permission>();
            string proc = "sysUsp_Role_GetMenuPermissions";
            var param = new DynamicParameters();
            param.Add("@roleCode", roleCode);

            var data = await _sqlDataAccess.LoadDataUsingStoredProcedure<RoleDto>(proc, param);
            if (data.Any())
            {
                returnData = data
                    .GroupBy(p => new { p.roleCode })
                    .Select(g => new Role_Permission
                    {
                        roleCode = g.Key.roleCode,
                        Permissions = g.Select(p => p.PermissionName)
                    })
                    .ToList();
            }

            return returnData;
        }
    }
}
