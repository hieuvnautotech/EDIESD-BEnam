using Microsoft.EntityFrameworkCore;
using ESD_EDI_BE.CustomAttributes;
using System.ComponentModel.DataAnnotations;

namespace ESD_EDI_BE.Models.Dtos.Common
{
    [JSGenerateAttribute]
    public class LoginModelDto
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        [Unicode(false)]
        public string? userName { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 1)]
        [Unicode(false)]
        public string? userPassword { get; set; }

        public bool? isOnApp { get; set; } = false;

        public LoginModelDto()
        {
            isOnApp = false;
        }
    }
}
