using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ESD_EDI_BE.Models.Dtos.Redis;

namespace ESD_EDI_BE.Models.Dtos.Common
{
    public class RoleMenuDto
    {
        public string roleCode { get; set; }
        public IEnumerable<RoleMenuRedis>? Menus { get; set; }
    }
}