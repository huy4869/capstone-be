using G24_BWallet_Backend.Models;
using G24_BWallet_Backend.Models.ObjectType;
using System.Threading.Tasks;

namespace G24_BWallet_Backend.Repository.Interface
{
    public interface IMemberRepository
    {
        Task<MemberDetailParam> ShowMemeberDetail(int eventId);
        Task<Event> GetEvent(int eventId);
    }
}
