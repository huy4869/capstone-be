using System.Collections.Generic;

namespace G24_BWallet_Backend.Models.ObjectType
{
    public class NewEvent
    {
        public Event Event { get; set; }
        public List<User> Members { get; set; }
    }
}
