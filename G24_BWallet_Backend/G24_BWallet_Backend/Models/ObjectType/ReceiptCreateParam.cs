using System.Collections.Generic;

namespace G24_BWallet_Backend.Models.ObjectType
{
    public class ReceiptCreateParam
    {
        public int EventID { get; set; }
        public int UserID { get; set; }
        public string ReceiptName { get; set; }
        public double ReceiptAmount { get; set; }
        public virtual List<UserDept> UserDepts { get; set; }
        public virtual User User { get; set; }
        public virtual Event Event { get; set; }
        public virtual List<string> IMGLinks { get; set; }
    }
}
