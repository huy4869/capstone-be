using G24_BWallet_Backend.Models;
using G24_BWallet_Backend.Models.ObjectType;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace G24_BWallet_Backend.Repository.Interface
{
    public interface IEventRepository
    {
        Task<List<EventHome>> GetAllEventsAsync(int userID);
        Task<int> AddEventAsync(Event e);
        Task AddEventMember(int eventID, List<int> memebers);
        Task<string> CreateEventUrl(int eventID);
        Task<bool> CheckUserJoinEvent(EventUserID eu);
        Task<string> GetEventUrl(int eventId);
    }
}
