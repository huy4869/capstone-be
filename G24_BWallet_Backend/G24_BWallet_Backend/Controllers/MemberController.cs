using G24_BWallet_Backend.Models.ObjectType;
using G24_BWallet_Backend.Repository.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Collections.Generic;
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
            MemberDetailParam m = await repo.ShowMemeberDetail(eventId, GetUserId());
            return new Respond<MemberDetailParam>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Show ra các thành viên trong event, " +
                "ai là kiểm duyệt, thu ngân, member",
                Data = m
            };
        }

        // phân quyền
        [HttpPost("promote")]
        public async Task<Respond<string>> PromoteMemberRole(EventUserIDRole e)
        {
            // kiểm tra xem có phải là owner không, nếu không phải thì không được phân quyền
            bool isOwner = await repo.IsOwner(e.EventId, GetUserId());
            if (isOwner)
            {
                await repo.PromoteMemberRole(e);
                if (e.Role == 2) // inspector
                    return new Respond<string>()
                    {
                        StatusCode = HttpStatusCode.Accepted,
                        Error = "",
                        Message = "Thêm người kiểm duyệt chứng từ thành công",
                        Data = "Thêm người kiểm duyệt chứng từ thành công"
                    };
                return new Respond<string>() // cashier
                {
                    StatusCode = HttpStatusCode.Accepted,
                    Error = "",
                    Message = "Thêm người thu ngân thành công",
                    Data = "Thêm người thu ngân thành công"
                };
            }
            return new Respond<string>()
            {
                StatusCode = HttpStatusCode.NotAcceptable,
                Error = "",
                Message = "Bạn không phải chủ sự kiện!",
                Data = null
            };
        }

        [HttpPost("delete-promote")]
        public async Task<Respond<string>> DeletePromoteMemberRole(EventUserID e)
        {
            // kiểm tra xem có phải là owner không, nếu không phải thì không được xoá
            bool isOwner = await repo.IsOwner(e.EventId, GetUserId());
            if (isOwner)
            {
                if(e.UserId == GetUserId())
                    return new Respond<string>()
                    {
                        StatusCode = HttpStatusCode.NotAcceptable,
                        Error = "",
                        Message = "Xoá phân quyền thất bại",
                        Data = "Xoá phân quyền thất bại"
                    };
                await repo.DeletePromoteMemberRole(e);
                return new Respond<string>()
                {
                    StatusCode = HttpStatusCode.Accepted,
                    Error = "",
                    Message = "Xoá phân quyền thành công",
                    Data = "Xoá phân quyền thành công"
                };
            }
            return new Respond<string>()
            {
                StatusCode = HttpStatusCode.NotAcceptable,
                Error = "",
                Message = "Bạn không phải chủ sự kiện!",
                Data = "Bạn không phải chủ sự kiện!"
            };
        }

        [HttpPost("inactive-member")]
        public async Task<Respond<string>> InActiveMember(EventUserID e)
        {
            // kiểm tra xem có phải là owner không, nếu không phải thì không được inactive
            bool isOwner = await repo.IsOwner(e.EventId, GetUserId());
            if (isOwner)
            {
                if (GetUserId() == e.UserId)
                {
                    return new Respond<string>()
                    {
                        StatusCode = HttpStatusCode.NotAcceptable,
                        Error = "",
                        Message = "Bạn không thể ẩn chính mình",
                        Data = null
                    };
                }
                int check = await repo.InActiveMember(e);
                if (check == 10)
                    return new Respond<string>()
                    {
                        StatusCode = HttpStatusCode.NotAcceptable,
                        Error = "",
                        Message = "Còn chứng từ chưa xử lý",
                        Data = "Còn chứng từ chưa xử lý"
                    };
                if (check == 11)
                    return new Respond<string>()
                    {
                        StatusCode = HttpStatusCode.NotAcceptable,
                        Error = "",
                        Message = "Còn khoản nợ chưa xử lý",
                        Data = "Còn khoản nợ chưa xử lý"
                    };
                return new Respond<string>()
                {
                    StatusCode = HttpStatusCode.Accepted,
                    Error = "",
                    Message = "Ẩn thành viên thành công",
                    Data = null
                };
            }
            return new Respond<string>()
            {
                StatusCode = HttpStatusCode.NotAcceptable,
                Error = "",
                Message = "Bạn không phải chủ sự kiện",
                Data = null
            };
        }

        // show ra role của member hiện tại trong event
        [HttpGet("role/eventId={eventId}")]
        public async Task<Respond<IDictionary>> GetMemberRole(int eventId)
        {
            IDictionary role = await repo.GetMemberRole(eventId, GetUserId());
            return new Respond<IDictionary>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Member role(trạng thái trong event hiện tại)",
                Data = role
            };
        }

        // show ra danh sách các member thường để promote, chỉ owner mới thấy
        [HttpGet("list-promote/eventId={eventId}")]
        public async Task<Respond<List<IdAvatarNamePhone>>> ListPromote(int eventId)
        {
            bool isOwner = await repo.IsOwner(eventId, GetUserId());
            if (isOwner == false)
            {
                return new Respond<List<IdAvatarNamePhone>>()
                {
                    StatusCode = HttpStatusCode.NotAcceptable,
                    Error = "",
                    Message = "Bạn không phải chủ sự kiện!",
                    Data = null
                };
            }
            List<IdAvatarNamePhone> list = await repo.ListPromote(eventId, GetUserId());
            return new Respond<List<IdAvatarNamePhone>>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Danh sách các thành viên có thể phân quyền",
                Data = list
            };
        }
    }
}
