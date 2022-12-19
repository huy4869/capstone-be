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
        [Column("PaidId")]
        public int PaidId { get; set; }

        [ForeignKey("userdept")]
        public int DebtId { get; set; }
        public double PaidAmount { get; set; }

        [ForeignKey("PaidId")]
        public virtual PaidDept PaidDept { get; set; }

        [ForeignKey("DebtId")]
        public virtual UserDept UserDept { get; set; }
    }
}
