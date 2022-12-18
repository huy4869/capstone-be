using G24_BWallet_Backend.Models;
using G24_BWallet_Backend.Models.ObjectType;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace G24_BWallet_Backend.Repository.Interface
{
    public interface IMemberRepository
    {
        Task<MemberDetailParam> ShowMemeberDetail(int eventId, int userId);
        Task<Event> GetEvent(int eventId);
        Task PromoteMemberRole(EventUserIDRole e);
        Task DeletePromoteMemberRole(EventUserID e);
        Task<bool> IsOwner(int eventId, int v);
        Task<int> InActiveMember(EventUserID e);
        Task<bool> IsInspector(int eventId, int userId);
        Task<bool> IsCashier(int eventId, int userId);
        Task<bool> IsNormalMember(int eventId, int userId);
        Task<IDictionary> GetMemberRole(int eventId, int userId);
        Task<int> GetRole(int eventId, int userId);
        Task<List<IdAvatarNamePhone>> ListPromote(int eventId, int v);
        Task<string> GetPhoneByUserId(int useriD);
        Task<List<EventUser>> SortList(List<EventUser> eventUsers);
    }
}
