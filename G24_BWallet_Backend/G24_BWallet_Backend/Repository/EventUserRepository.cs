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

        public async Task<List<Member>> SearchEventUsersAsync(int eventID, int userID, string name = null)
        {
            List<Member> result = new List<Member>();
            List<Member> eventUsers;
            
            /*var whoSearch = myDB.Users.Include(u => u.Account).Where(u => u.ID == userID)
                .Select(u => new Member
                {
                    UserId = u.ID,
                    UserName = u.UserName,
                    UserAvatar = u.Avatar,
                    UserPhone = u.Account.PhoneNumber
                })
                .FirstOrDefault();
            result.Add(whoSearch);*/

            if (name == null)//find all
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

            result.AddRange(eventUsers);
            return result;
        }
    }
}
