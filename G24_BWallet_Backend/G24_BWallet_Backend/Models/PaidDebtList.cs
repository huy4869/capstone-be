using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace G24_BWallet_Backend.Models
{
    [Table("paiddebtlist")]
    public class PaidDebtList
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [ForeignKey("paiddept")]
        public int PaidId { get; set; }
        [ForeignKey("userdept")]
        public int DebtId { get; set; }
        public double PaidAmount { get; set; }

        public virtual PaidDept PaidDept { get; set; }
        public virtual UserDept UserDept { get; set; }
    }
}
