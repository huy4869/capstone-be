using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace G24_BWallet_Backend.Models
{
    [Table("Receipt")]
    public class Receipt
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserID { get; set; }
        [ForeignKey("Event")]
        public int EventID { get; set; }

        public string ReceiptName { get; set; }
        public int ReceiptStatus { get; set; }
        public double ReceiptAmount { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public virtual List<UserDept> UserDepts { get; set; }
        public virtual User User { get; set; }
        public virtual Event Event { get; set; }
    }
}