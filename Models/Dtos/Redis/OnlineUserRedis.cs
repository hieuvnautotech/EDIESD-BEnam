using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ESD_EDI_BE.Extensions;

namespace ESD_EDI_BE.Models.Dtos.Redis
{
    public class OnlineUserRedis
    {
        public long userId { get; set; }
        public string? userName { get; set; }
        public string? fullName { get; set; }
        public string? lastLoginOnWeb { get; set; }
        public string? lastLoginOnApp { get; set; }

        public OnlineUserRedis()
        {
            userId = AutoId.AutoGenerate();
            userName = string.Empty;
            fullName = string.Empty;
            lastLoginOnWeb = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            lastLoginOnApp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}