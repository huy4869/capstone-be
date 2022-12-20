using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace G24_BWallet_Backend.Models
{
    [Table("user")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public string UserName { get; set; }
        public string Avatar { get; set; }
        public int AllowAddFriendStatus { get; set; }
        public int AllowInviteEventStatus { get; set; }

        [ForeignKey("account")]
        public int AccountID { get; set; }
        public virtual Account? Account { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        

        public User()
        {
        }

        public User(int iD, string userName, string avatar, string phone)
        {
            ID = iD;
            UserName = userName;
            Avatar = avatar;
            Account = new Account();
            Account.PhoneNumber = phone;
        }
    }
}
