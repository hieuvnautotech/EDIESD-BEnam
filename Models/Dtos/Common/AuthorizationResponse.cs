using ESD_EDI_BE.CustomAttributes;

namespace ESD_EDI_BE.Models.Dtos.Common
{
    [JSGenerateAttribute]
    public class AuthorizationResponse
    {
        public string? accessToken { get; set; } = default;
        public string? refreshToken { get; set; } = default;
        public bool isSuccess { get; set; } = true;
        public string? reason { get; set; } = default; 
    }
}
