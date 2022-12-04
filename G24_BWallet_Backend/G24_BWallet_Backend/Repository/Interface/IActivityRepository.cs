using G24_BWallet_Backend.Models.ObjectType;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace G24_BWallet_Backend.Repository.Interface
{
    public interface IActivityRepository
    {
        Task<List<ActivityScreen>> GetActivity(int userId);
        Task AddActivity(int userId,string content,string iconType);
    }
}
