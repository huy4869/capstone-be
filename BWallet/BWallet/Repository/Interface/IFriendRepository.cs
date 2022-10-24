using BWallet.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BWallet.Repository.Interface
{
    public interface IFriendRepository
    {
        public Task<IQueryable<User>> GetFriendsAsync(int userID);
    }
}
