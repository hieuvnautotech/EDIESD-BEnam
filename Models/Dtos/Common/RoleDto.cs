using Microsoft.EntityFrameworkCore;
using ESD_EDI_BE.CustomAttributes;
using System.ComponentModel.DataAnnotations;

namespace ESD_EDI_BE.Models.Dtos.Common
{
    [JSGenerate]
    public class RoleDto : BaseModel
    {
        public long roleId { get; set; }
        public string? roleCode { get; set; }
        [Required]
        [StringLength(50)]
        public string? roleName { get; set; } = default;
        public string? roleDescription { get; set; } = default;
        public IEnumerable<MenuDto>? Menus { get; set; } = default;

        public string? PermissionName { get; set; } = default;
        public IEnumerable<long>? MenuIds { get; set; } = default;
        // public IEnumerable<RoleMenu>? RoleMenus { get; set; } = default;
        public IEnumerable<RoleMissingMenuPermission>? MissingMenuPermissions { get; set; } = default;
    }

    [JSGenerate]
    public class RoleDeleteDto
    {
        public long roleId { get; set; }
        public string roleCode { get; set; }
        public IEnumerable<MenuDto>? Menus { get; set; } = default;
        public long? createdBy { get; set; } = default;
        public List<long>? menuPermissionIds { get; set; }
        public List<long>? menuIds { get; set; }
        public long menuId { get; set; }
    }

    [JSGenerate]
    public class RoleMissingMenuPermission
    {
        public long Id { get; set; }
        public string MP_Description { get; set; } = string.Empty;
        public string roleCode { get; set; } = string.Empty;
    }

    [JSGenerate]
    public class Role_Permission
    {
        public string? roleCode { get; set; }
        public IEnumerable<string?> Permissions { get; set; } = new List<string>();
    }

    // [JSGenerate]
    // public class RoleMenu
    // {
    //     public long menuId { get; set; }
    //     public long? parentId { get; set; } = default;
    //     public string? menuName { get; set; } = string.Empty;
    //     public string? menuComponent { get; set; } = string.Empty;
    //     public string? menuIcon { get; set; } = string.Empty;
    //     public string? languageKey { get; set; } = string.Empty;
    //     public string? navigateUrl { get; set; } = string.Empty;
    //     public byte? menuLevel { get; set; } = byte.MinValue;
    //     public bool forRoot { get; set; } = false;
    //     public bool? forApp { get; set; } = false;
    // }
}
