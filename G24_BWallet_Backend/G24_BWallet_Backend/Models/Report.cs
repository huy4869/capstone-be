using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace G24_BWallet_Backend.Models
{
    [Table("report")]
    public class Report
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        [ForeignKey("event")]
        public int EventId { get; set; }
        [ForeignKey("receipt")]
        public int ReceiptId { get; set; }
        [ForeignKey("user")]
        public int UserId { get; set; }

        public string ReportReason { get; set; }
        public int ReportStatus { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public virtual User User { get; set; }
        public virtual Receipt Receipt { get; set; }

    }
}
