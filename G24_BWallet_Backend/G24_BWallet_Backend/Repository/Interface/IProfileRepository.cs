using G24_BWallet_Backend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace G24_BWallet_Backend.Repository.Interface
{
    public interface IProfileRepository
    {
        Task<List<Request>> GetInvitePending(int userId);
        Task<List<Request>> GetRequestPending(int userId);
        Task<User> GetUserById(int userId);
    }
}
