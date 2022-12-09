using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace G24_BWallet_Backend.Models
{
    [Table("event")]
    public class Event
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        [Required]
        public string EventName { get; set; }
        public string EventLogo { get; set; }
        public string EventDescript { get; set; }
        public string EventLink { get; set; }
        public int EventStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

    }
}
