using System.Collections.Generic;

namespace Capstone_API.Models.ObjectType
{
    public class NewEvent
    {
        public string EventName { get; set; }
        public string EventDescript { get; set; }
        public List<User> Members { get; set; }
    }
}
