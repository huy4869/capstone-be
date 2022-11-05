using System;

namespace G24_BWallet_Backend.Models.ObjectType
{
    public class ReceiptMainInfo
    {
        public int Id { get; set; }
        public string ReceiptName { get; set; }
        public double ReceiptAmount { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
