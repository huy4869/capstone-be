using G24_BWallet_Backend.DBContexts;
using G24_BWallet_Backend.Models;
using G24_BWallet_Backend.Repository.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace G24_BWallet_Backend.Repository
{
    public class EventRepository : IEventRepository
    {
        private readonly MyDBContext context;

        public EventRepository(MyDBContext myDB)
        {
            this.context = myDB;
        }

        public async Task<int> AddEventAsync(Event e)
        {
            e.EventStatus = 1;
            e.CreatedAt = System.DateTime.Now;
            e.UpdatedAt = System.DateTime.Now;
            await context.Events.AddAsync(e);
            await context.SaveChangesAsync();
            return e.ID;
        }

        public async Task AddEventMember(int eventID, List<int> memebers)
        {
            int count = 0;
            foreach (var item in memebers)
            {
                EventUser eu = new EventUser();
                eu.EventID = eventID;
                eu.UserID = item;
                eu.UserRole = (count == 0) ? 1 : 2;
                count = 1;
                await context.EventUsers.AddAsync(eu);
            }
            await context.SaveChangesAsync();
        }

        public async Task<List<Event>> GetAllEventsAsync([FromBody] int userID)
        {
            //List<Event> events = await myDB.Events.ToListAsync();
            //List<EventUser> eventUsers = await myDB.EventUsers.
            //    Where(e => e.UserID == userID).ToListAsync();

            //var query =
            //from e in events 
            //join eu in eventUsers on e.ID equals eu.EventID
            //select e;
            var query = await context.EventUsers
                .Where(eu => eu.UserID == userID)
                .Select(eu => eu.Events).ToListAsync();
            return query;
        }

    }
}
