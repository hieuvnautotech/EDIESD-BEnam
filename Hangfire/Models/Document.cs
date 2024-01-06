using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ESD_EDI_BE.Hangfire.Models
{
    [Table("sysTbl_Document")]
    public class Document
    {
        [Key]
        public long documentId { get; set; }

        [StringLength(50)]
        [Unicode(false)]
        public string menuComponent { get; set; }

        [StringLength(255)]
        public string urlFile { get; set; }

        [StringLength(50)]
        [Unicode(false)]
        public string documentLanguage { get; set; }

        public bool? isActived { get; set; }

        public byte[] row_version { get; set; }

        public bool? transToCustomer { get; set; }
    }
}