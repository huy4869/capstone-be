using System;
using System.Collections.Generic;

namespace G24_BWallet_Backend.Models.ObjectType
{
    public class ReceiptDetail
    {
        public int Id { get; set; }
        public int OwnerId { get; set; }
        public string OwnerName { get; set; }
        public string ReceiptName { get; set; }
        public int ReceiptStatus { get; set; }
        public double ReceiptAmount { get; set; }
        public string ReceiptAmountFormat { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> IMGLinks { get; set; }
        public List<ReceiptDetailDept> ListUserDepts { get; set; }
        
    }

    public class ReceiptDetailDept
    {
        public int DeptId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Avatar { get; set; }
        public double DebtLeft { get; set; }
        public string DebtLeftFormat { get; set; }
    }
}
