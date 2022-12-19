using G24_BWallet_Backend.Models;
using G24_BWallet_Backend.Models.ObjectType;
using G24_BWallet_Backend.Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace G24_BWallet_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileRepository repo;

        public ProfileController(IProfileRepository repository)
        {
            repo = repository;
        }

        protected int GetUserId()
        {
            return int.Parse(this.User.Claims.First(i => i.Type == "UserId").Value);
        }

        [HttpGet]
        public async Task<Respond<IDictionary>> GetProfile()
        {
            var userID = GetUserId();
            User user = await repo.GetUserById(userID);
            List<Request> requestPending = await repo.GetRequestPending(userID);
            List<Invite> invitePending = await repo.GetInvitePending(userID);
            // 2 cái list trên là đang lấy hết, ở đây mình chỉ cần đếm các list có status
            // là đang chờ thôi
            int requestCount = 0;
            requestPending.ForEach(r =>
            {
                if (r.Status == 3) requestCount++;
            });
            int inviteCount = 0;
            invitePending.ForEach(i =>
            {
                if (i.Status == 0) inviteCount++;
            });
            IDictionary<string, object> dictionary = new Dictionary<string, object>();
            dictionary.Add("Avatar", user.Avatar);
            dictionary.Add("UserName", user.UserName);
            dictionary.Add("AllowAddFriendStatus", user.AllowAddFriendStatus);
            dictionary.Add("AllowInviteEventStatus", user.AllowInviteEventStatus);
            dictionary.Add("PhoneNumber", user.Account.PhoneNumber);
            dictionary.Add("RequestPending", requestCount);
            dictionary.Add("InvitePending", inviteCount);
            return new Respond<IDictionary>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Lấy thông tin người dùng và yêu cầu tham gia, lời mời vào nhóm",
                Data = (IDictionary)dictionary
            };
        }

        [HttpGet("request")]
        public async Task<Respond<List<RequestJoinParam>>> ShowRequestJoinEvent()
        {
            var userID = GetUserId();
            List<RequestJoinParam> list = await repo.ShowRequestJoinEvent(userID);
            return new Respond<List<RequestJoinParam>>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Danh sách trạng thái các yêu cầu tham gia nhóm của user hiện tại",
                Data = list
            };
        }

        [HttpGet("invite")]
        public async Task<Respond<List<InviteJoinParam>>> ShowInviteJoinEvent()
        {
            var userID = GetUserId();
            List<InviteJoinParam> list = await repo.ShowInviteJoinEvent(userID);
            return new Respond<List<InviteJoinParam>>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Danh sách trạng thái các lời mời tham gia nhóm của user hiện tại",
                Data = list
            };
        }

        [HttpPost("respond")]
        public async Task<Respond<string>> RespondInvite(InviteRespondParam i)
        {
            var userID = GetUserId();
            var isAccept = await repo.ChangeStatusInvite(i, userID);
            if (isAccept == false)
                return new Respond<string>()
                {
                    StatusCode = HttpStatusCode.NotAcceptable,
                    Error = "",
                    Message = "Từ chối tham gia event này",
                    Data = null
                };
            return new Respond<string>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Đồng ý tham gia event này",
                Data = null
            };
        }

        // đếm số lời mời tham gia vào nhóm mà mình chưa accept, status = 0
        [HttpGet("invite-count")]
        public async Task<Respond<int>> CountInviteRequest()
        {
            var userID = GetUserId();
            List<Invite> invitePending = await repo.GetInvitePending(userID);
            int inviteCount = 0;
            invitePending.ForEach(i =>
            {
                if (i.Status == 0) inviteCount++;
            });
            return new Respond<int>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Đếm số lời mời tham gia vào nhóm mà mình chưa đồng ý( hoặc từ chối)",
                Data = inviteCount
            };
        }
    }
}
