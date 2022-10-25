using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace G24_BWallet_Backend.Models
{
    [Table("User")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        [Required]
        public string UserName { get; set; }
        public string FBlink { get; set; }
        public string BankInfo { get; set; }
        [ForeignKey("Account")]
        public int AccountID { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
        [Required]
        public DateTime UpdatedAt { get; set; }

        //public virtual Account Account { get; set; }
        public ICollection<Friend> Friends { get; set; }
    }
}
