using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace G24_BWallet_Backend.Models
{
    [Table("receipt")]
    public class Receipt
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReceiptID { get; set; }

        [ForeignKey("UserID")]
        public User User { get; set; }
        [ForeignKey("EventID")]
        public Event Event { get; set; }


        public string ReceiptName { get; set; }
        public string ReceiptPicture { get; set; }
        public int DivideType { get; set; }
        public int ReceiptStatus { get; set; }
        public double ReceiptAmount { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }
        [Required]
        public DateTime UpdatedAt { get; set; }

    }
}
