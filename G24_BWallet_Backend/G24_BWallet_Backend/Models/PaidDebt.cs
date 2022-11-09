﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace G24_BWallet_Backend.Models
{
    [Table("paiddept")]
    public class PaidDept
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [ForeignKey("user")]
        public int UserId { get; set; }
        [ForeignKey("event")]
        public int EventId { get; set; }
        public string PaidProof { get; set; }
        public double TotalMoney { get; set; }
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public virtual User User { get; set; }
        public virtual Event Event { get; set; }

    }
}
