using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace G24_BWallet_Backend.Models
{
    [Table("friend")]
    public class Friend
    {
        [ForeignKey("user")]
        [Column(Order = 1)]
        public int UserID { get; set; }
        [ForeignKey("user")]
        [Column(Order = 2)]
        public int UserFriendID { get; set; }
        public int status { get; set; }
        public DateTime CreatedAt { get; set; }
        
    }
}
