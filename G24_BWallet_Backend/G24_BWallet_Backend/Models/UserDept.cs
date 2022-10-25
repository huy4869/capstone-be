using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace G24_BWallet_Backend.Models
{
    [Table("user_dept")]
    public class UserDept
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DeptId { get; set; }
        [ForeignKey("UserID")]
        public User User { get; set; }
        [ForeignKey("ReceiptID")]
        public Receipt Receipt { get; set; }
        [Required]
        public int DeptStatus { get; set; }
        [Required]
        public double Debit { get; set; }
    }
}
