using G24_BWallet_Backend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace G24_BWallet_Backend.Repository.Interface
{
    public interface IEventRepository
    {
        Task<List<Event>> GetAllEventsAsync(int userID);
        Task<int> AddEventAsync(Event e);
        Task AddEventMember(int eventID, List<User> memebers);
    }
}
