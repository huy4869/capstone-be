using G24_BWallet_Backend.DBContexts;
using G24_BWallet_Backend.Models;
using G24_BWallet_Backend.Models.ObjectType;
using G24_BWallet_Backend.Repository.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace G24_BWallet_Backend.Repository
{
    public class EventUserRepository : IEventUserRepository
    {
        private readonly MyDBContext myDB;

        public EventUserRepository(MyDBContext myDB)
        {
            this.myDB = myDB;
        }
        public EventUserRepository()
        {
        }

        public async Task<List<Member>> GetAllEventUsersAsync(int eventID)
        {
            var eventUsers = await myDB.EventUsers
                .Include(eu => eu.User).Include(eu => eu.User.Account)
                .Where(eu => eu.EventID == eventID)
                .Select(eu => new Member
                {
                    UserId = eu.UserID,
                    UserName = eu.User.UserName,
                    UserAvatar = eu.User.Avatar,
                    UserPhone = eu.User.Account.PhoneNumber
                })
                .ToListAsync();
            
            return eventUsers;
        }

        public string test(int eventID)
        {
            return "fuk you" + eventID;
        }
    }
}
