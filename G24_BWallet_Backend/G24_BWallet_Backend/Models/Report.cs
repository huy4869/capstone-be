using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace G24_BWallet_Backend.Models
{
    [Table("Report")]
    public class Report
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        [ForeignKey("Event")]
        public int EventId { get; set; }
        [ForeignKey("Receipt")]
        public int ReceiptId { get; set; }
        [ForeignKey("User")]
        public int UserId { get; set; }

        public string ReportReason { get; set; }
        public int ReportStatus { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public virtual User User { get; set; }
        public virtual Receipt Receipt { get; set; }

    }
}
