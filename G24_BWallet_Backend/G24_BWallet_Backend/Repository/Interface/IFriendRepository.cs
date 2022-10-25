using G24_BWallet_Backend.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace G24_BWallet_Backend.Repository.Interface
{
    public interface IFriendRepository
    {
        public Task<IQueryable<User>> GetFriendsAsync(int userID);
    }
}
