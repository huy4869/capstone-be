using BWallet.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BWallet.Repository.Interface
{
    public interface IEventRepository
    {
        Task<List<Event>> GetAllEventsAsync(int userID);
        Task<int> AddEventAsync(string eventName, string eventDes);
        Task AddEventMember(int eventID, List<User> memebers);
    }
}
