using Capstone_API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Capstone_API.Repository.Interface
{
    public interface IEventRepository
    {
        Task<List<Event>> GetAllEventsAsync(User user);
        Task<List<User>> GetAllUsersAsync();
    }
}
