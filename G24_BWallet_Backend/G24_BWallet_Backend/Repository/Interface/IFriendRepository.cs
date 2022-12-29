using G24_BWallet_Backend.Models;
using G24_BWallet_Backend.Models.ObjectType;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace G24_BWallet_Backend.Repository.Interface
{
    public interface IFriendRepository
    {
        Task<List<Member>> SearchFriendToInvite(int userID,int eventId, string phone = null);
        Task AddInvite(EventFriendParam e);

        public Task<List<Member>> GetFriendsAsync(int userID, string phone = null);
        Task<List<Member>> GetListFriendRequest(int UserID, string phone = null);
        
        Task<List<searchFriendToAdd>> SearchFriendToAdd(int userID, string phone = null);
        Task<string> SendFriendRequestAsync(int userID, int friendID);
        Task<string> AcceptFriendRequestAsync(int yourID, Friend respone);

        Task<string> DeleteFriendAsync(int userID, int friendID);
        
    }
}
