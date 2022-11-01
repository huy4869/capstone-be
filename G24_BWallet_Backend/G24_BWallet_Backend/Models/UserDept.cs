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
        [ForeignKey("User")]
        public int UserID { get; set; }
        [ForeignKey("Receipt")]
        public int ReceiptID { get; set; }
        [Required]
        public int DeptStatus { get; set; }
        public double Debit { get; set; }

        public virtual User User { get; set; }

        public virtual Receipt Receipt { get; set; }
    }
}
