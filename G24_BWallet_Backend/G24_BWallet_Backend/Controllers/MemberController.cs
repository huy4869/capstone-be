using G24_BWallet_Backend.Models.ObjectType;
using G24_BWallet_Backend.Repository.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace G24_BWallet_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MemberController : ControllerBase
    {
        private readonly IMemberRepository repo;

        public MemberController(IMemberRepository member)
        {
            repo = member;
        }

        protected int GetUserId()
        {
            return int.Parse(this.User.Claims.First(i => i.Type == "UserId").Value);
        }

        // show ra các thành viên trong event, ai là kiểm duyệt, thu ngân, member
        [HttpGet("eventId={eventId}")]
        public async Task<Respond<MemberDetailParam>> ShowAllMemeberAndRole(int eventId)
        {
            MemberDetailParam m = await repo.ShowMemeberDetail(eventId);
            return new Respond<MemberDetailParam>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Show ra các thành viên trong event, " +
                "ai là kiểm duyệt, thu ngân, member",
                Data = m
            };
        }

        [HttpPost("promote")]
        public async Task<Respond<string>> PromoteMemberRole(EventUserIDRole e)
        {
            // kiểm tra xem có phải là owner không, nếu không phải thì không được phân quyền
            bool isOwner = await repo.IsOwner(e.EventId, GetUserId());
            if (isOwner)
            {
                await repo.PromoteMemberRole(e);
                return new Respond<string>()
                {
                    StatusCode = HttpStatusCode.Accepted,
                    Error = "",
                    Message = "Phân quyền cho member thành cashier hoặc kiểm duyệt thành công",
                    Data = null
                };
            }
            return new Respond<string>()
            {
                StatusCode = HttpStatusCode.NotAcceptable,
                Error = "",
                Message = "Phân quyền cho member thất bại vì bạn không phải là owner",
                Data = null
            };
        }

        [HttpPost("delete-promote")]
        public async Task<Respond<string>> DeletePromoteMemberRole(EventUserID e)
        {
            // kiểm tra xem có phải là owner không, nếu không phải thì không được xoá
            bool isOwner = await repo.IsOwner(e.EventId, e.UserId);
            if (isOwner == false)
            {
                await repo.DeletePromoteMemberRole(e);
                return new Respond<string>()
                {
                    StatusCode = HttpStatusCode.Accepted,
                    Error = "",
                    Message = "Xoá phân quyền cho member thành công",
                    Data = null
                };
            }
            return new Respond<string>()
            {
                StatusCode = HttpStatusCode.NotAcceptable,
                Error = "",
                Message = "Không xoá được phân quyền vì bạn không phải là owner",
                Data = null
            };
        }

        [HttpPost("remove-member")]
        public async Task<Respond<string>> RemoveMember(EventUserID e)
        {
            // kiểm tra xem có phải là owner không, nếu không phải thì không được xoá
            bool isOwner = await repo.IsOwner(e.EventId,GetUserId());
            if (isOwner)
            {
                if(GetUserId() == e.UserId) {
                    return new Respond<string>()
                    {
                        StatusCode = HttpStatusCode.NotAcceptable,
                        Error = "",
                        Message = "Bạn không thể xoá chính mình",
                        Data = null
                    };
                }
                await repo.RemoveMember(e);
                return new Respond<string>()
                {
                    StatusCode = HttpStatusCode.Accepted,
                    Error = "",
                    Message = "Xoá member khỏi event thành công",
                    Data = null
                };
            }
            return new Respond<string>()
            {
                StatusCode = HttpStatusCode.NotAcceptable,
                Error = "",
                Message = "Bạn không phải owner của event nên không xoá được member",
                Data = null
            };
        }
    }
}
