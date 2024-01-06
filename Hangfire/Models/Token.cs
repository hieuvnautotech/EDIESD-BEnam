using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ESD_EDI_BE.Hangfire.Models
{
    [Table("sysTbl_RefreshToken")]
    public class Token
    {
        [Key]
        public long refreshTokenId { get; set; }

        [Required]
        [Unicode(false)]
        public string refreshToken { get; set; }

        [Required]
        [Unicode(false)]
        public string accessToken { get; set; }

        [Precision(0)]
        public DateTime? createdDate { get; set; }

        [Precision(0)]
        public DateTime? expiredDate { get; set; }

        [StringLength(50)]
        [Unicode(false)]
        public string ipAddress { get; set; }

        public bool? isValidated { get; set; }

        public long userId { get; set; }

        public bool? isOnApp { get; set; }
    }
}