using System.Collections.Generic;

namespace G24_BWallet_Backend.Models.ObjectType
{
    public class EventHome
    {
        public int EventId { get; set; }
        public string EventName { get; set; }
        public string EventLogo { get; set; }
        public double TotalMoney { get; set; }
        public List<UserHome> ListUser { get; set; }
    }

    public class UserHome
    {
        public string UserName { get; set; }
        public string Avatar { get; set; }
        public double Money { get; set; }
        public string MoneyColor { get; set; }
    }
}
