using G24_BWallet_Backend.Models;
using G24_BWallet_Backend.Models.ObjectType;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace G24_BWallet_Backend.Repository.Interface
{
    public interface IFriendRepository
    {
        public Task<IQueryable<Member>> GetFriendsAsync(int userID);
    }
}
