using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace G24_BWallet_Backend.Models
{
    [Table("PaidDebtList")]
    public class PaidDebtList
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [ForeignKey("PaidDept")]
        public int PaidId { get; set; }
        [ForeignKey("UserDept")]
        public int DebtId { get; set; }
        public double PaidAmount { get; set; }

        public virtual PaidDept PaidDept { get; set; }
        public virtual UserDept UserDept { get; set; }
    }
}
