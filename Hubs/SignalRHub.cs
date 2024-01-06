using Microsoft.AspNetCore.SignalR;
using ESD_EDI_BE.Models.Dtos.Common;
using ESD_EDI_BE.Services.Cache;
using ESD_EDI_BE.Services.Common;
using Newtonsoft.Json;
using ESD_EDI_BE.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ESD_EDI_BE.Models;
using Microsoft.EntityFrameworkCore;
using ESD_EDI_BE.Extensions;

namespace ESD_EDI_BE.Hubs
{
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class SignalRHub : Hub
    {

        private readonly ISysCacheService _sysCacheService;
        // private readonly ESD_DBContext _dbContext;
        private readonly IJwtService _jwtService;
        private readonly IUserService _userService;


        public SignalRHub(
            ISysCacheService sysCacheService
            // , ESD_DBContext dbContext
            , IJwtService jwtService
            , IUserService userService
            )
        {
            _sysCacheService = sysCacheService;
            // _dbContext = dbContext;
            _jwtService = jwtService;
            _userService = userService;
        }

        public async Task SendMessage(string message)
        {
            if (!string.IsNullOrEmpty(message) && Clients != null)
            {
                //RabbitMQ_Model messageObject = JsonConvert.DeserializeObject<RabbitMQ_Model>(message);
                await Clients.All.SendAsync("ReceiveEDIMessage", message);
            }
        }

        public async Task SetUserLoggedIn(UserDto user)
        {
            // var res = await _sysCacheService.GetOnlineUsersFromRedis();
            // var user = res.Find(x => x.userId == userId);
            if (Clients != null)
            {
                if (Clients.Group(user.userId.ToString()) != null)
                {
                    await Clients.Group(user.userId.ToString()).SendAsync("ReceivedLoggedInUser", user);
                }
            }
        }

        public async Task SendUserRoleUpdate(string token, string? userId)
        {
            var res = await _jwtService.CheckRoleChanged(token);
            if (res ?? true)
            {
                if (Clients != null)
                {
                    if (Clients.Group(userId) != null)
                    {
                        await Clients.Group(userId).SendAsync("ReceivedUserRoleUpdate");
                    }
                }
            }
        }

        public async Task SendRoleDelete(string roleCode)
        {
            if (Clients != null)
            {
                if (Clients.Group($"{CommonConst.GROUP_ROLE}_{roleCode.Trim()}") != null)
                {
                    await Clients.Group($"{CommonConst.GROUP_ROLE}_{roleCode.Trim()}").SendAsync("ReceivedUserRoleUpdate");
                }
            }
        }

        public async Task SendRoleMenusUpdate(string roleCode)
        {
            var data = await _sysCacheService.GetRoleMenusFromRedis(roleCode);
            if (Clients != null)
            {
                if (Clients.Group($"{CommonConst.GROUP_ROLE}_{roleCode.Trim()}") != null)
                {
                    await Clients.Group($"{CommonConst.GROUP_ROLE}_{roleCode.Trim()}").SendAsync("ReceivedMenusOfRole", new
                    {
                        roleCode,
                        Menus = data
                    });
                }
            }
        }

        public async Task SendUpdateRoleMissingPermissions(string roleCode)
        {
            var data = await _sysCacheService.GetRoleMissingMenuPermissionsFromRedis(roleCode);
            var result = data
                        .GroupBy(o => o.roleCode)
                        .Select(g => new UserMissingPermissionByRole
                        {
                            roleCode = g.Key,
                            MissingPermissions = g.Select(o => o.MP_Description).ToList()
                        }).FirstOrDefault();
            if (Clients != null)
            {
                if (Clients.Group($"{CommonConst.GROUP_ROLE}_{roleCode.Trim()}") != null)
                {
                    await Clients.Group($"{CommonConst.GROUP_ROLE}_{roleCode.Trim()}").SendAsync("UpdateRoleMissingPermissions", result ?? new UserMissingPermissionByRole()
                    {
                        roleCode = roleCode,
                        MissingPermissions = new List<string>()
                    });
                }
            }
        }

        public override async Task OnConnectedAsync()
        {
            string? token = Context?.GetHttpContext()?.Request.Query["access_token"];

            var userId = await _jwtService.ValidateTokenAsync(token);
            await Groups.AddToGroupAsync(Context?.ConnectionId, userId);

            var userRole = _jwtService.GetRolenameFromToken(token);

            if (!string.IsNullOrWhiteSpace(userRole))
            {
                string[] roleArray = userRole.Replace(" ", "").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (roleArray.Length > 0)
                {
                    foreach (var roleCode in roleArray)
                    {
                        await Groups.AddToGroupAsync(Context?.ConnectionId, $"{CommonConst.GROUP_ROLE}_{roleCode.Trim()}");
                    }
                }
            }

            await base.OnConnectedAsync();
            await MakeUserLogout();
            await SendUserRoleUpdate(token, userId);
            await SendUserMenus(long.Parse(userId));
            await SendUserMissingPermissionGroupByRole(long.Parse(userId));
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }

        private async Task MakeUserLogout()
        {
            if (Clients != null)
                await Clients.Group(CommonConst.NON_EXIST_TOKEN).SendAsync("MakeUserLogout");
        }

        private async Task SendUserMissingPermissionGroupByRole(long userId)
        {
            var userRoleCodelist = await _jwtService.GetUserRoleCodeListByUserId(userId);
            var data = await _userService.GetUserMissingPermissionGroupByRole(userRoleCodelist);
            if (data.Any())
            {
                if (Clients != null)
                {
                    if (Clients.Group(userId.ToString()) != null)
                    {
                        await Clients.Group(userId.ToString()).SendAsync("UserMissingPermissionGroupByRole", data);
                    }
                }
            }
        }

        private async Task SendUserMenus(long userId)
        {
            var userRoleCodelist = await _jwtService.GetUserRoleCodeListByUserId(userId);
            var data = await _userService.GetUserMenuGroupByRole(userRoleCodelist);
            if (data.Any())
            {
                if (Clients != null)
                {
                    if (Clients.Group(userId.ToString()) != null)
                    {
                        await Clients.Group(userId.ToString()).SendAsync("ReceivedUserMenus", data);
                    }
                }
            }
        }
    }
}
