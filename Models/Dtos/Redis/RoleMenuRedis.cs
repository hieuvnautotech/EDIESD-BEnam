using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ESD_EDI_BE.Extensions;

namespace ESD_EDI_BE.Models.Dtos.Redis
{
    public class RoleMenuRedis
    {
        public long menuId { get; set; }
        public long? parentId { get; set; }
        public string? menuName { get; set; }
        public byte? menuLevel { get; set; }
        public byte? sortOrder { get; set; }
        public string? menuIcon { get; set; }
        public string? languageKey { get; set; }
        public string? menuComponent { get; set; }
        public string? navigateUrl { get; set; }
        public bool forRoot { get; set; }
        public bool? forApp { get; set; }
        public string? roleCode { get; set; }

        public RoleMenuRedis()
        {
            menuId = AutoId.AutoGenerate();
            parentId = default;
            menuName = default;
            menuLevel = default;
            sortOrder = default;
            menuIcon = default;
            languageKey = default;
            menuComponent = default;
            navigateUrl = default;
            forRoot = true;
            forApp = default;
            roleCode = default;
        }
    }
}