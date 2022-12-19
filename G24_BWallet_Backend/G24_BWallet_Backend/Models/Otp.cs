using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace G24_BWallet_Backend.Models
{
    [Table("otpcode")]
    public class Otp
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OtpID { get; set; }
        public string Phone { get; set; }
        public string JWToken { get; set; }
        public string OtpCode { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
