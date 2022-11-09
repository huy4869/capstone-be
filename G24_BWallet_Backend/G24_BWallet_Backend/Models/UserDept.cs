using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace G24_BWallet_Backend.Models
{
    [Table("userdept")]
    public class UserDept
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [ForeignKey("user")]
        public int UserId { get; set; }
        [ForeignKey("receipt")]
        public int ReceiptId { get; set; }
        public int DeptStatus { get; set; }
        public double Debt { get; set; }
        public double DebtLeft { get; set; }

        public virtual User User { get; set; }

        public virtual Receipt Receipt { get; set; }
    }
}