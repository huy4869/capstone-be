using Capstone_API.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Capstone_API.Repository.Interface
{
    public interface IFriendRepository
    {
        public Task<IQueryable<User>> GetFriendsAsync(int userID);
    }
}
