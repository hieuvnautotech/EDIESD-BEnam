using ESD_EDI_BE.Models.Dtos.Common;

namespace ESD_EDI_BE.Models.Dtos
{
    public class PPORTAL_QUAL02_POLICYDto : BaseModel
    {
        public long Id { get; set; }
        public string? ITEM_CODE { get; set; }
        public string? TRAND_TP { get; set; }
        public int? CTQ_NO { get; set; }
        public decimal MIN_VALUE { get; set; }
        public decimal MAX_VALUE { get; set; }
        public string? TRAND_TP_NAME { get; set; }
    }
}