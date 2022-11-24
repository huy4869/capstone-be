using G24_BWallet_Backend.Models;
using G24_BWallet_Backend.Models.ObjectType;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace G24_BWallet_Backend.Repository.Interface
{
    public interface IProfileRepository
    {
        Task<bool> ChangeStatusInvite(InviteRespondParam i,int userId);
        Task<List<Request>> GetInvitePending(int userId);
        Task<List<Request>> GetRequestPending(int userId);
        Task<User> GetUserById(int userId);
        Task<List<InviteJoinParam>> ShowInviteJoinEvent(int userID);
        Task<List<RequestJoinParam>> ShowRequestJoinEvent(int userID);
    }
}
