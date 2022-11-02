using G24_BWallet_Backend.DBContexts;
using G24_BWallet_Backend.Models;
using G24_BWallet_Backend.Models.ObjectType;
using G24_BWallet_Backend.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace G24_BWallet_Backend.Repository
{
    public class FriendRepository : IFriendRepository
    {
        private readonly MyDBContext context;

        public FriendRepository(MyDBContext myDB)
        {
            this.context = myDB;
        }

        public async Task<IQueryable<Member>> GetFriendsAsync(int userID)
        {
            var list = from u in context.Users
                       join f in context.Friends
                       on u.ID equals f.UserID
                       where u.ID == 4
                       select f;
            var list2 = from u in context.Users.Include(u => u.Account)
                        join l in list
                        on u.ID equals l.UserFriendID
                        select (new Member(u.ID, u.UserName, u.Avatar,u.Account.PhoneNumber));
            return await Task.FromResult(list2);
        }
    }
}
