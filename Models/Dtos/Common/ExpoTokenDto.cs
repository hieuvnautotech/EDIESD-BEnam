using System.ComponentModel.DataAnnotations;
using ESD_EDI_BE.CustomAttributes;

namespace ESD_EDI_BE.Models.Dtos.Common
{
    [JSGenerateAttribute]
    public class ExpoTokenDto
    {
        public string ExpoToken { get; set; }
        public string DeviceName { get; set; }
        public string DeviceModelName { get; set; }
        public string DeviceBrand { get; set; }
        public string DeviceType { get; set; }
        public bool? isActived { get; set; }
    }
}
