using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace G24_BWallet_Backend.Models
{
    [Table("activity")]
    public class Activity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        [Column(Order = 1)]
        [ForeignKey("user")]
        public int UserID { get; set; }
        [Column(Order = 2)]
        [ForeignKey("activityicon")]
        public int? ActivityIconId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }


        public virtual User User { get; set; }
        public virtual ActivityIcon ActivityIcon { get; set; }
    }
}
