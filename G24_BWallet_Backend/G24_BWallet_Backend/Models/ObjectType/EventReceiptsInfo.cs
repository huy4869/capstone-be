using System;
using System.Collections.Generic;

namespace G24_BWallet_Backend.Models.ObjectType
{
    public class EventReceiptsInfo
    {
        public int Id { get; set; }
        public string EventLogo { get; set; }
        public int EventStatus { get; set; }
        public string EventName { get; set; }

        public double TotalAmount { get; set; }
        public string TotalAmountFormat { get; set; }
        public double GroupAmount { get; set; }
        public string GroupAmountFormat { get; set; }
        public double UserAmount { get; set; }
        public string UserAmountFormat { get; set; }
        //public double ReceiveAmount { get; set; }
        public string ReceiveAmountFormat { get; set; }
        //public double DebtAmount { get; set; }
        public string DebtAmountFormat { get; set; }
        public int Number { get; set; }
        //public double TotalReceiptsAmount { get; set; }
        public List<ReceiptMainInfo> listReceipt { get; set; }
    }
}
