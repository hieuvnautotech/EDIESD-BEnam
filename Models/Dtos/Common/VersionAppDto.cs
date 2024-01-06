using Microsoft.EntityFrameworkCore;
using ESD_EDI_BE.CustomAttributes;
using System.ComponentModel.DataAnnotations;

namespace ESD_EDI_BE.Models.Dtos.Common
{
    [JSGenerateAttribute]
    public class VersionAppDto
    {
        public long id_app { get; set; }
        public string app_code { get; set; }
        public string name_file { get; set; } = string.Empty;
        //public int? version { get; set; }
        public string app_version { get; set; }
        public string CHPlay_version { get; set; }
        public string link_url { get; set; }
        public int update_type { get; set; } = 0;
        public string change_date { get; set; } = string.Empty;
        public bool newVersion { get; set; }
        public IFormFile? file { get; set; }
        public long? createdBy { get; set; } = default;
        public byte[] row_version { get; set; }

    }
}
