using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace G24_BWallet_Backend.Models
{
    [Table("UserDept")]
    public class UserDept
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [ForeignKey("User")]
        public int UserId { get; set; }
        [ForeignKey("Receipt")]
        public int ReceiptId { get; set; }
        public int DeptStatus { get; set; }
        public double Debt { get; set; }
        public double DebtLeft { get; set; }

        public virtual User User { get; set; }

        public virtual Receipt Receipt { get; set; }
    }
}
