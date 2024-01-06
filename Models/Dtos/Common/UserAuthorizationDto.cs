using ESD_EDI_BE.CustomAttributes;

namespace ESD_EDI_BE.Models.Dtos.Common
{
    [JSGenerateAttribute]
    public class UserAuthorizationDto
    {
        public long userId { get; set; }
        public string userName { get; set; } = string.Empty;
        public string roleName { get; set; } = string.Empty;
        public string permissionName { get; set; } = string.Empty;
    }
}
