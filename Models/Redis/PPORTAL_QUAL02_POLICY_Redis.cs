using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ESD_EDI_BE.Models.Redis
{
    public class PPORTAL_QUAL02_POLICY_Redis
    {
        public long Id { get; set; }
        public string? ITEM_CODE { get; set; }
        public string? TRAND_TP { get; set; }
        public int? CTQ_NO { get; set; }
        public decimal MIN_VALUE { get; set; }
        public decimal MAX_VALUE { get; set; }
    }
}