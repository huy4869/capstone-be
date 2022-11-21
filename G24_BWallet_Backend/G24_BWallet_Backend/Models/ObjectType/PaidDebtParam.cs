using System.Collections.Generic;

namespace G24_BWallet_Backend.Models.ObjectType
{
    public class PaidDebtParam
    {
        public int UserId { get; set; }
        public int EventId { get; set; }
        public string PaidImage { get; set; }
        public double TotalMoney { get; set; }
        public List<RenamePaidDebtList> ListEachPaidDebt { get; set; }
    }

    public class RenamePaidDebtList
    {
        public int userDeptId { get; set; }
        public int debtLeft { get; set; }
    }
}
