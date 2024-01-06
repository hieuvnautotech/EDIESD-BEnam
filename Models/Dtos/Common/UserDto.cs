using Microsoft.EntityFrameworkCore;
using ESD_EDI_BE.CustomAttributes;
using System.ComponentModel.DataAnnotations;
using ESD_EDI_BE.Models.Dtos.Redis;

namespace ESD_EDI_BE.Models.Dtos.Common
{
    [JSGenerateAttribute]
    public class UserDto : BaseModel
    {
        public long userId { get; set; }

        [StringLength(100)]
        [Unicode(false)]
        public string? userName { get; set; }
        [StringLength(100)]
        [Unicode(false)]
        public string? fullName { get; set; }

        [StringLength(100)]
        public string? userPassword { get; set; }

        [StringLength(100)]
        public string? newPassword { get; set; }

        public string? lastLoginOnWeb { get; set; }

        public string? lastLoginOnApp { get; set; }

        //// Role list
        public IEnumerable<RoleDto>? Roles { get; set; }
        public IEnumerable<string>? RoleNames { get; set; }
        public string? RoleNameList { get; set; }
        public string? RoleCodeList { get; set; }

        /// <summary>
        /// UserPermission
        /// </summary>
        public IEnumerable<string>? PermissionNames { get; set; }

        //Menu List
        public IEnumerable<RoleMenuRedis>? Menus { get; set; }

        public IEnumerable<string>? MissingPermissions { get; set; }

        //Token
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public bool? isOnline { get; set; }
        public IEnumerable<UserMissingPermissionByRole>? MissingPermissionByRole { get; set; }
    }

    public class UserMissingPermissionByRole
    {
        public string roleCode { get; set; }
        public IEnumerable<string>? MissingPermissions { get; set; }
    }
}
