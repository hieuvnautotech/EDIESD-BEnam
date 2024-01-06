using System.ComponentModel.DataAnnotations;
using ESD_EDI_BE.CustomAttributes;

namespace ESD_EDI_BE.Models.Dtos.Common
{
    [JSGenerateAttribute]
    public class MenuPermissionDto : BaseModel
    {
        public long Id { get; set; }
        public string MP_Name { get; set; }
        public string MP_Description { get; set; }
        public long menuId { get; set; }
        public string? languageKey { get; set; }
        public long permissionId { get; set; }
        public string permissionName { get; set; }
        public string photo { get; set; }
        public bool? forRoot { get; set; }
        public string RabbitMQType { get; set; }
        public IEnumerable<MenuPermissionMap>? MenuPermissionMaps { get; set; }
    }

    [JSGenerateAttribute]
    public class MenuPermissionMap
    {
        public long MenuId { get; set; }
        public IEnumerable<long>? Ids { get; set; }
    }
}
