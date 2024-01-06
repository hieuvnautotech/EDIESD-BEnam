using System.ComponentModel.DataAnnotations;
using ESD_EDI_BE.CustomAttributes;

namespace ESD_EDI_BE.Models.Dtos.Common
{
    [JSGenerateAttribute]
    public class CommonMasterDto : BaseModel
    {
        public string commonMasterCode { get; set; }
        public string commonMasterName { get; set; }
        public bool forRoot { get; set; }

    }
}
