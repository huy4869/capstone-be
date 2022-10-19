using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Capstone_API.Models
{
    [Table("EventUser")]
    public class EventUser
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EventUserID { get; set; }
        [ForeignKey("UserID")]
        public User User { get; set; }
        [ForeignKey("EventID")]
        public Event Event { get; set; }
        [Required]
        public int UserRole { get; set; }
    }
}
