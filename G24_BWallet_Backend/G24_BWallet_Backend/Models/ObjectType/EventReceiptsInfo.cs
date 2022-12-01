﻿using System;
using System.Collections.Generic;

namespace G24_BWallet_Backend.Models.ObjectType
{
    public class EventReceiptsInfo
    {
        public int Id { get; set; }
        public string EventLogo { get; set; }
        public int EventStatus { get; set; }
        public string EventName { get; set; }

        public MoneyColor UserInvolveAmount { get; set; }

        //public double TotalReceiptsAmount { get; set; }

        public List<ReceiptMainInfo> listReceipt { get; set; }
    }
}
