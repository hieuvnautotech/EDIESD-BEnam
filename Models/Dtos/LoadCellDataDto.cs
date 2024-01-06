using System.ComponentModel.DataAnnotations;

namespace ESD_EDI_BE.Models.Dtos
{
    public class LoadCellDataDto
    {
        [Required]
        public string ESDMachineCode { get; set; }

        [Required]
        public IFormFile? file { get; set; }
    }
}
