using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace G24_BWallet_Backend.Models
{
    [Table("eventuser")]
    public class EventUser
    {
        [Column(Order = 1)]
        [ForeignKey("user")]
        public int UserID { get; set; }
        public virtual User User { get; set; }


        [Column(Order = 2)]
        [ForeignKey("event")]
        public int EventID { get; set; }
        public int UserRole { get; set; }
        public virtual Event Event { get; set; }
    }
}
