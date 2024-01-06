using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ESD_EDI_BE.Models.Dtos
{
    public class LogDto
    {
        public string LogMessage { get; set; } = string.Empty;
        public string LogType { get; set; } = string.Empty;
        public string? ItemCode { get; set; } = null;
        public string? LogTime { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        public int? NumberOfMeasurements { get; set; } = 0;
    }
}