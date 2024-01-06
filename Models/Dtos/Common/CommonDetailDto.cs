using System.ComponentModel.DataAnnotations;
using ESD_EDI_BE.CustomAttributes;

namespace ESD_EDI_BE.Models.Dtos.Common
{
    [JSGenerate]
    public class CommonDetailDto : BaseModel
    {
        //public long id
        //{
        //    get
        //    {
        //        return commonDetailId;
        //    }

        //}
        public long commonDetailId { get; set; }
        public string? commonMasterCode { get; set; }
        public string? commonDetailCode { get; set; }
        public string? commonDetailName { get; set; } = default;

    }
}
