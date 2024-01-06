using Microsoft.EntityFrameworkCore;
using ESD_EDI_BE.CustomAttributes;
using System.ComponentModel.DataAnnotations;

namespace ESD_EDI_BE.Models.Dtos.Common
{
    [JSGenerate]
    public class DocumentDto
    {
        public long? documentId { get; set; }
        public string menuComponent { get; set; } = string.Empty;
        public string urlFile { get; set; } = string.Empty;
        public string documentLanguage { get; set; } = "EN";
        public bool? isActived { get; set; }
        public byte[] row_version { get; set; }
        public bool? transToCustomer { get; set; }
        public IFormFile? file { get; set; }
        public string menuName { get; set; } = string.Empty;
        public string languageKey { get; set; } = string.Empty;
        public string RabbitMQType { get; set; }
    }
}
