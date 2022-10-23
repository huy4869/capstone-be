using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Capstone_API.Models
{
    [Table("EventUser")]
    public class EventUser
    {
        [Column(Order = 1)]
        [ForeignKey("User")]
        public int UserID { get; set; }
        [Column(Order = 2)]
        [ForeignKey("Event")]
        public int EventID { get; set; }
        [Required]
        public int UserRole { get; set; }
        public virtual Event Events { get; set; }
    }
}
