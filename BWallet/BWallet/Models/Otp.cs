﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BWallet.Models
{
    [Table("OtpCode")]
    public class Otp
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OtpID { get; set; }
        [Required]
        public string Phone { get; set; }
        [Required]
        public string JWToken { get; set; }
        [Required]
        public string OtpCode { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
    }
}
