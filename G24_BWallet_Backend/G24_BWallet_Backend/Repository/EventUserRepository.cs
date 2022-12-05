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

        public async Task<searchEventMember> SearchEventUsersAsync(int eventID, int userID, string name = null)
        {
            var whoSearch = myDB.Users.Include(u => u.Account).Where(u => u.ID == userID).FirstOrDefault();
            List<Member> eventUsers;

            if(name == null)//find all
            eventUsers = await myDB.EventUsers
                .Include(eu => eu.User).Include(eu => eu.User.Account)
                .Where(eu => eu.EventID == eventID && eu.UserID != userID)
                .Select(eu => new Member
                {
                    UserId = eu.UserID,
                    UserName = eu.User.UserName,
                    UserAvatar = eu.User.Avatar,
                    UserPhone = eu.User.Account.PhoneNumber
                })
                .ToListAsync();

            else//find by name
            eventUsers = await myDB.EventUsers
                .Include(eu => eu.User).Include(eu => eu.User.Account)
                .Where(eu => eu.EventID == eventID && eu.UserID != userID && eu.User.UserName.Contains(name))
                .Select(eu => new Member
                {
                    UserId = eu.UserID,
                    UserName = eu.User.UserName,
                    UserAvatar = eu.User.Avatar,
                    UserPhone = eu.User.Account.PhoneNumber
                })
                .ToListAsync();

            searchEventMember result = new searchEventMember();
            result.UserId = userID;
            result.UserName = whoSearch.UserName;
            result.UserAvatar = whoSearch.Avatar;
            result.UserPhone = whoSearch.Account.PhoneNumber;
            result.eventUsers = eventUsers;
            return result;
        }
    }
}
