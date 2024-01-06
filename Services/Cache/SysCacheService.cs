using Microsoft.Extensions.Options;
using ESD_EDI_BE.Cache;
using ESD_EDI_BE.Extensions;
using ESD_EDI_BE.Models.Dtos.Common;
using ESD_EDI_BE.Services.Common;
using static ESD_EDI_BE.Extensions.ServiceExtensions;
using ESD_EDI_BE.Services.EDI;
using ESD_EDI_BE.Models.Dtos;
using FluentValidation.TestHelper;
using ESD_EDI_BE.Models.Dtos.Redis;
using ESD_EDI_BE.Models.Redis;

namespace ESD_EDI_BE.Services.Cache
{
    public interface ISysCacheService
    {
        bool Del(string key);

        Task DelAsync(string[] key);

        Task<bool> DelByPatternAsync(string key);

        bool Set(string key, object value);

        bool Set(string key, object value, TimeSpan expire);

        Task<bool> SetAsync(string key, object value);

        string Get(string key);

        Task<string> GetAsync(string key);

        T Get<T>(string key);

        Task<T> GetAsync<T>(string key);

        /// <summary>
        /// Online User
        /// </summary>
        // Task<List<UserDto>> GetOnlineUsersFromRedis();

        // Task SetOnlineUserToRedis(UserDto data);

        // Task SetOnlineUsersToRedis();

        /// <summary>
        /// Policies
        /// </summary>
        Task<List<PPORTAL_QUAL02_POLICY_Redis>> GetPoliciesFromRedis();

        Task SetPoliciesToRedis();

        Task SetPoliciesToRedis(PPORTAL_QUAL02_POLICY_Redis model);

        Task RemovePoliciesFromRedis(PPORTAL_QUAL02_POLICYDto model);

        /// <summary>
        /// Available Tokens
        /// </summary>
        Task<HashSet<string>> GetAvailableTokensFromRedis();

        Task SetAvailableTokensToRedis(HashSet<string> tokens);

        Task SetAvailableTokensToRedis();

        Task AddAvailableTokenToRedis(string token);

        Task RemoveAvailableTokenFromRedis(string token);

        /// <summary>
        /// Menus of Role
        /// </summary>
        Task<IEnumerable<RoleMenuRedis>> GetRoleMenusFromRedis(string roleCode);

        Task SetRoleMenusToRedis();

        Task SetRoleMenusToRedis(string roleCode);

        /// <summary>
        /// Missing Menu's Permissions of Role
        /// </summary>
        Task<IEnumerable<RoleMissingMenuPermission>> GetRoleMissingMenuPermissionsFromRedis(string roleCode);

        Task SetRoleMissingMenuPermissionsToRedis();

        Task SetRoleMissingMenuPermissionsToRedis(string roleCode);

        /// <summary>
        /// Menu's Permissions of Role
        /// </summary>
        Task<IEnumerable<string?>> GetRoleMenuPermissionsFromRedis(string roleCode);

        Task SetRoleMenuPermissionsToRedis();

        Task SetRoleMenuPermissionsToRedis(string roleCode);
    }

    // [ScopedRegistration]
    [SingletonRegistration]
    public class SysCacheService : ISysCacheService
    {
        private readonly ICache _cache;
        // private readonly ILoginService _loginService;
        private readonly IPolicyService _policyService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IRoleService _roleService;
        public SysCacheService(
            ICache cache
            // , ILoginService loginService
            , IPolicyService policyService
            , IRefreshTokenService refreshTokenService
            , IRoleService roleService
            )
        {
            _cache = cache;
            // _loginService = loginService;
            _policyService = policyService;
            _refreshTokenService = refreshTokenService;
            _roleService = roleService;
        }

        public bool Del(string key)
        {
            _cache.Del(key);
            return true;
        }

        public async Task DelAsync(string[] key)
        {
            await _cache.DelAsync(key);
            // return Task.FromResult(true);
        }

        public Task<bool> DelByPatternAsync(string key)
        {
            _cache.DelByPatternAsync(key);
            return Task.FromResult(true);
        }

        public bool Set(string key, object value)
        {
            return _cache.Set(key, value);
        }

        public bool Set(string key, object value, TimeSpan expire)
        {
            return _cache.Set(key, value, expire);
        }

        public async Task<bool> SetAsync(string key, object value)
        {
            return await _cache.SetAsync(key, value);
        }

        public string Get(string key)
        {
            return _cache.Get(key);
        }

        public T Get<T>(string key)
        {
            return _cache.Get<T>(key);
        }

        public async Task<string> GetAsync(string key)
        {
            return await _cache.GetAsync(key);
        }

        public Task<T> GetAsync<T>(string key)
        {
            return _cache.GetAsync<T>(key);
        }

        public Task<bool> ExistsAsync(string key)
        {
            return _cache.ExistsAsync(key);
        }

        /// <summary>
        /// Online User
        /// </summary>
        // public async Task SetOnlineUsersToRedis()
        // {
        //     var cacheKey = CommonConst.CACHE_KEY_ONLINE_USERS;
        //     var data = await _loginService.GetOnlineUsers();

        //     await _cache.SetAsync(cacheKey, AutoMapperConfig<UserDto, OnlineUserRedis>.MapList(data.Data.ToList()));
        // }

        // private async Task SetOnlineUsersToRedis(List<UserDto> data)
        // {
        //     var cacheKey = CommonConst.CACHE_KEY_ONLINE_USERS;
        //     await _cache.SetAsync(cacheKey, data);
        // }

        // public async Task SetOnlineUserToRedis(UserDto data)
        // {
        //     var users = await GetOnlineUsersFromRedis();
        //     var index = users.FindIndex(x => x.userId == data.userId);
        //     if (index == -1) users.Add(data);
        //     else users[index] = data;
        //     await SetOnlineUsersToRedis(users);
        // }

        // public async Task<List<UserDto>> GetOnlineUsersFromRedis()
        // {
        //     var cacheKey = CommonConst.CACHE_KEY_ONLINE_USERS;
        //     var data = await _cache.GetOrCreateAsync(cacheKey, async () =>
        //     {
        //         var data = await _loginService.GetOnlineUsers();
        //         var users = data.Data != null ? data.Data.ToList() : new List<UserDto>();
        //         await SetOnlineUsersToRedis(users);
        //         return users;
        //     });
        //     return data;
        // }

        /// <summary>
        /// Policies
        /// </summary>
        public async Task SetPoliciesToRedis()
        {
            var cacheKey = CommonConst.CACHE_KEY_POLICIES;
            var data = await _policyService.GetForCache();

            await _cache.SetAsync(cacheKey, data.Data);
        }

        private async Task SetPoliciesToRedis(List<PPORTAL_QUAL02_POLICY_Redis?> data)
        {
            var cacheKey = CommonConst.CACHE_KEY_POLICIES;
            try
            {
                await _cache.SetAsync(cacheKey, data);
            }
            catch (Exception ex)
            {
                throw;
            }
            //await _cache.SetAsync(cacheKey, data);
        }

        public async Task SetPoliciesToRedis(PPORTAL_QUAL02_POLICY_Redis model)
        {
            var policies = await GetPoliciesFromRedis();
            var index = policies.FindIndex(x => x.Id == model.Id);
            if (index == -1) policies.Add(model);
            else policies[index] = model;
            await SetPoliciesToRedis(policies);
        }

        public async Task<List<PPORTAL_QUAL02_POLICY_Redis>> GetPoliciesFromRedis()
        {
            try
            {
                var cacheKey = CommonConst.CACHE_KEY_POLICIES;
                var data = await _cache.GetOrCreateAsync(cacheKey, async () =>
                {
                    var data = await _policyService.GetForCache();
                    var policies = data.Data.ToList();
                    await SetPoliciesToRedis(policies);
                    return policies;
                });
                return data;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task RemovePoliciesFromRedis(PPORTAL_QUAL02_POLICYDto model)
        {
            try
            {
                var policies = await GetPoliciesFromRedis();
                policies.RemoveAll(item => item.Id == model.Id);
                await SetPoliciesToRedis(policies);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        /// <summary>
        /// Available Tokens
        /// </summary>
        public async Task SetAvailableTokensToRedis()
        {
            var cacheKey = CommonConst.CACHE_AVAILABLE_TOKENS;
            var data = await _refreshTokenService.GetAvailables();

            await _cache.SetAsync(cacheKey, data);
        }

        public async Task SetAvailableTokensToRedis(HashSet<string> tokens)
        {
            var cacheKey = CommonConst.CACHE_AVAILABLE_TOKENS;
            await _cache.SetAsync(cacheKey, tokens);
        }

        public async Task<HashSet<string>> GetAvailableTokensFromRedis()
        {
            var cacheKey = CommonConst.CACHE_AVAILABLE_TOKENS;
            var data = await _cache.GetOrCreateAsync(cacheKey, async () =>
            {
                var availableTokens = await _refreshTokenService.GetAvailables();
                await SetAvailableTokensToRedis(availableTokens);
                return availableTokens;
            });
            return data;
        }

        public async Task AddAvailableTokenToRedis(string token)
        {
            var tokens = await GetAvailableTokensFromRedis();
            if (!tokens.Contains(token)) tokens.Add(token);

            await SetAvailableTokensToRedis(tokens);
        }

        public async Task RemoveAvailableTokenFromRedis(string token)
        {
            var tokens = await GetAvailableTokensFromRedis();
            tokens.Remove(token);

            await SetAvailableTokensToRedis(tokens);
        }

        /// <summary>
        /// Menus of Role
        /// </summary>
        public async Task SetRoleMenusToRedis()
        {
            var cacheKey = CommonConst.CACHE_KEY_ROLE_MENU;

            var roleList = await _roleService.GetForSelect(new List<string> { "000" });
            if (roleList.Data.Any())
            {
                var data = await _roleService.GetMenusByRoleCode(null);

                // Convert data to a dictionary for faster lookups
                var roleDtoDictionary = data.ToDictionary(x => x.roleCode);

                var tasks = roleList.Data.Select(async item =>
                {
                    if (roleDtoDictionary.TryGetValue(item.roleCode, out var roleDto))
                    {
                        await _cache.SetAsync($"{cacheKey}_{roleDto.roleCode}", AutoMapperConfig<MenuDto, RoleMenuRedis>.MapList(roleDto.Menus.ToList()));
                    }
                    else
                    {
                        await _cache.SetAsync($"{cacheKey}_{item.roleCode}", new List<RoleMenuRedis>());
                    }
                });

                // Wait for all tasks to complete
                await Task.WhenAll(tasks);
            }
        }

        public async Task SetRoleMenusToRedis(string roleCode)
        {
            var cacheKey = $"{CommonConst.CACHE_KEY_ROLE_MENU}_{roleCode}";
            var data = await _roleService.GetMenusByRoleCode(roleCode);

            if (data != null && data.Any() && data.First().Menus.Any())
            {
                var menus = AutoMapperConfig<MenuDto, RoleMenuRedis>.MapList(data.First().Menus.ToList());
                await _cache.SetAsync(cacheKey, menus);
            }
            else
            {
                await _cache.SetAsync(cacheKey, new List<RoleMenuRedis>());
            }
        }

        public async Task<IEnumerable<RoleMenuRedis>> GetRoleMenusFromRedis(string roleCode)
        {
            if (!string.IsNullOrEmpty(roleCode))
            {
                var cacheKey = $"{CommonConst.CACHE_KEY_ROLE_MENU}_{roleCode}";
                var data = await _cache.GetOrCreateAsync(cacheKey, async () =>
                {
                    var listRoleDto = await _roleService.GetMenusByRoleCode(roleCode);
                    if (listRoleDto.FirstOrDefault() != null)
                    {
                        var menusOfRole = AutoMapperConfig<MenuDto, RoleMenuRedis>.MapList(listRoleDto.FirstOrDefault().Menus.ToList());
                        await _cache.SetAsync($"{cacheKey}", menusOfRole);
                        return menusOfRole;
                    }
                    return new List<RoleMenuRedis>();
                });
                return data;
            }
            return new List<RoleMenuRedis>();
        }

        /// <summary>
        /// Missing Menu's Permissions of Role
        /// </summary>
        public async Task SetRoleMissingMenuPermissionsToRedis()
        {
            var cacheKey = CommonConst.CACHE_KEY_ROLE_MISSING_MENU_PERMISSION;
            await _cache.SetAsync($"{cacheKey}_000", new List<RoleMissingMenuPermission>());

            var data = await _roleService.GetMissingMenuPermissions(null);
            if (data.Any())
            {
                foreach (var item in data)
                {
                    await _cache.SetAsync($"{cacheKey}_{item.roleCode}", item.MissingMenuPermissions);
                }
            }
        }

        public async Task SetRoleMissingMenuPermissionsToRedis(string roleCode)
        {
            var cacheKey = CommonConst.CACHE_KEY_ROLE_MISSING_MENU_PERMISSION;

            var data = await _roleService.GetMissingMenuPermissions(roleCode);
            if (data.Any())
            {
                foreach (var item in data)
                {
                    await _cache.SetAsync($"{cacheKey}_{roleCode}", item.MissingMenuPermissions);
                }
            }
        }

        public async Task<IEnumerable<RoleMissingMenuPermission>> GetRoleMissingMenuPermissionsFromRedis(string roleCode)
        {
            var cacheKey = $"{CommonConst.CACHE_KEY_ROLE_MISSING_MENU_PERMISSION}_{roleCode}";
            var data = await _cache.GetOrCreateAsync(cacheKey, async () =>
            {
                if (roleCode == "000")
                {
                    await _cache.SetAsync($"{cacheKey}", new List<RoleMissingMenuPermission>());
                    return new List<RoleMissingMenuPermission>();
                }

                var listRoleDto = await _roleService.GetMissingMenuPermissions(roleCode);
                if (listRoleDto.FirstOrDefault() != null)
                {
                    var roleMissingPermission = listRoleDto.FirstOrDefault().MissingMenuPermissions;
                    await _cache.SetAsync($"{cacheKey}", roleMissingPermission);
                    return roleMissingPermission;
                }
                return new List<RoleMissingMenuPermission>();
            });
            return data;
        }

        public async Task SetRoleMenuPermissionsToRedis()
        {
            var cacheKey = CommonConst.CACHE_KEY_ROLE_PERMISSIONS;
            var roleList = await _roleService.GetForSelect(new List<string> { "000" });

            if (roleList.Data.Any())
            {
                var data = await _roleService.GetMenuPermissions(null);

                // Convert data to a dictionary for faster lookups
                var roleDtoDictionary = data.ToDictionary(x => x.roleCode);

                var tasks = roleList.Data.Select(async item =>
                {
                    if (roleDtoDictionary.TryGetValue(item.roleCode, out var roleDto))
                    {
                        await _cache.SetAsync($"{cacheKey}_{item.roleCode}", roleDto.Permissions);
                    }
                    else
                    {
                        await _cache.SetAsync($"{cacheKey}_{item.roleCode}", new List<string>());
                    }
                });

                // Wait for all tasks to complete
                await Task.WhenAll(tasks);
            }

            // var data = await _roleService.GetMenuPermissions(null);
            // if (data.Any())
            // {
            //     foreach (var item in data)
            //     {
            //         await _cache.SetAsync($"{cacheKey}_{item.roleCode}", item.Permissions);
            //     }
            // }
        }

        public async Task SetRoleMenuPermissionsToRedis(string roleCode)
        {
            var cacheKey = $"{CommonConst.CACHE_KEY_ROLE_PERMISSIONS}_{roleCode}";
            var data = await _roleService.GetMenuPermissions(roleCode);
            if (data.FirstOrDefault() != null)
            {
                await _cache.SetAsync($"{cacheKey}", data.FirstOrDefault().Permissions);
            }
            else
            {
                await _cache.SetAsync($"{cacheKey}", new List<string>());
            }
        }

        public async Task<IEnumerable<string?>> GetRoleMenuPermissionsFromRedis(string roleCode)
        {
            var cacheKey = $"{CommonConst.CACHE_KEY_ROLE_PERMISSIONS}_{roleCode}";
            var data = await _cache.GetOrCreateAsync(cacheKey, async () =>
            {
                var item = await _roleService.GetMenuPermissions(roleCode);
                var result = item.FirstOrDefault();
                if (result != null)
                {
                    await _cache.SetAsync($"{cacheKey}", result.Permissions);
                    return result.Permissions;
                }
                return new List<string>();
            });
            return data;
        }
    }
}
