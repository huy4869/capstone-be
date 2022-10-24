using System.Collections.Generic;

namespace BWallet.Models.ObjectType
{
    public class NewEvent
    {
        public string EventName { get; set; }
        public string EventDescript { get; set; }
        public List<User> Members { get; set; }
    }
}
