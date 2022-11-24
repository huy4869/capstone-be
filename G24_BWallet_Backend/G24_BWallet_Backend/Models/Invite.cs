using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace G24_BWallet_Backend.Models
{
    [Table("Invite")]
    public class Invite
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        [ForeignKey("User")]
        [Column(Order = 1)]
        public int UserID { get; set; }
        [ForeignKey("User")]
        [Column(Order = 2)]
        public int FriendId { get; set; }
        [ForeignKey("Event")]
        public int EventID { get; set; }
        public int Status { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime UpdateAt { get; set; }

        public virtual Event Event { get; set; }
    }
}
