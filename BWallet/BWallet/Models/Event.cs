using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace BWallet.Models
{
    [Table("Event")]
    public class Event
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        [Required]
        public string EventName { get; set; }
        [Required]
        public string EventLogo { get; set; }
        [Required]
        public string EventDescript { get; set; }
        [Required]
        public string EventLink { get; set; }
        [Required]
        public int EventStatus { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
        [Required]
        public DateTime UpdatedAt { get; set; }

    }
}
