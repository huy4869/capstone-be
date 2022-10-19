using Capstone_API.DBContexts;
using Capstone_API.Models;
using Capstone_API.Repository.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Capstone_API.Repository
{
    public class EventRepository : IEventRepository
    {
        private readonly MyDBContext myDB;

        public EventRepository(MyDBContext myDB)
        {
            this.myDB = myDB;
        }

        public async Task<List<Event>> GetAllEventsAsync([FromBody]User user)
        {
            List<Event> events = await myDB.Events.ToListAsync();
            List<EventUser> eventUsers = await myDB.EventUsers.
                Where(e => e.User.UserID == user.UserID).ToListAsync();

            var query =
            from e in events 
            join eu in eventUsers on e.EventID equals eu.Event.EventID
            select e;
            return query.ToList();
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await myDB.Users.ToListAsync();
        }
    }
}
