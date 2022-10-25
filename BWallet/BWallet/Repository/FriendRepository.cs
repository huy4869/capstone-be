using BWallet.DBContexts;
using BWallet.Models;
using BWallet.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BWallet.Repository
{
    public class FriendRepository : IFriendRepository
    {
        private readonly MyDBContext context;

        public FriendRepository(MyDBContext myDB)
        {
            this.context = myDB;
        }

        public async Task<IQueryable<User>> GetFriendsAsync(int userID)
        {
            var list = from u in context.Users
                       join f in context.Friends
                       on u.ID equals f.UserID
                       where u.ID == userID
                       select f;
            var list2 = from u in context.Users
                        join l in list
                        on u.ID equals l.UserFriendID
                        select u;
            return await Task.FromResult(list2);
        }
    }
}
