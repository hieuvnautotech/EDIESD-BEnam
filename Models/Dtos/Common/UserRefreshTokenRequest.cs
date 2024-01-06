using System.ComponentModel.DataAnnotations;
using ESD_EDI_BE.CustomAttributes;

namespace ESD_EDI_BE.Models.Dtos.Common
{
    [JSGenerateAttribute]
    public class UserRefreshTokenRequest
    {
        [Required]
        public string? expiredToken { get; set; }
        [Required]
        public string? refreshToken { get; set; }
        public string? ipAddress { get; set; }
    }
}
