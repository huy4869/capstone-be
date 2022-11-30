using G24_BWallet_Backend.Models;
using G24_BWallet_Backend.Models.ObjectType;
using G24_BWallet_Backend.Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace G24_BWallet_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FriendController : ControllerBase
    {
        private readonly IFriendRepository repo;

        public FriendController(IFriendRepository friendRepository)
        {
            repo = friendRepository;
        }
        protected int GetUserId()
        {
            return int.Parse(this.User.Claims.First(i => i.Type == "UserId").Value);
        }

        [HttpGet("add-member")]
        public async Task<Respond<List<Member>>> ListFriendAddToEvent([FromQuery] string phone)
        {
            int UserId = GetUserId();
            var list = repo.SearchFriendToInvite(UserId, phone);
            return new Respond<List<Member>>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Danh sách người mời vào nhóm",
                Data = await list
            };
        }

        [HttpPost("add-member")]
        public async Task<Respond<string>> AddFriendToEvent(EventFriendParam e)
        {
            e.UserId = GetUserId();
            await repo.AddInvite(e);
            return new Respond<string>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Đã mời bạn bè vào event này, chờ họ tham gia",
                Data = null
            };
        }

        [HttpGet]
        public async Task<Respond<IEnumerable<Member>>> GetFriends([FromQuery] string phonenumber)
        {
            int userId = GetUserId();
            var friends = repo.GetFriendsAsync(userId, phonenumber);
            return new Respond<IEnumerable<Member>>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Get friends success",
                Data = await friends
            };
        }

        [HttpGet("search-friend")]
        public async Task<Respond<IEnumerable<Member>>> SearchAddFriend([FromQuery] string phonenumber)
        {
            int userId = GetUserId();
            var friends = repo.SearchFriendToAdd(userId, phonenumber);
            return new Respond<IEnumerable<Member>>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "search friend to add",
                Data = await friends
            };
        }

        [HttpGet("friend-request")]
        public async Task<Respond<List<Member>>> GetFriendRequest([FromQuery] string phonenumber)
        {
            int userId = GetUserId();
            var sResult = repo.GetListFriendRequest(userId, phonenumber);
            return new Respond<List<Member>>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "",
                Data = await sResult
            };
        }
        [HttpPost("friend-request")]
        public async Task<Respond<string>> SendFriendRequest([FromBody]Friend friend)
        {
            int userId = GetUserId();
            var sendRequest = repo.SendFriendRequestAsync(userId, friend.UserFriendID);
            return new Respond<string>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = await sendRequest + "",
                Data = ""
            };
        }

        [HttpPost("response-request")]
        public async Task<Respond<string>> AcceptFriendRequest([FromBody] Friend friend)
        {
            int userId = GetUserId();
            var sendRequest = repo.AcceptFriendRequestAsync(userId, friend);
            return new Respond<string>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = await sendRequest + "",
                Data = null
            };
        }

        [HttpDelete("{friendID}")]
        public async Task<Respond<string>> DeleteFriend(int friendID)
        {
            int userId = GetUserId();
            var result = repo.DeleteFriendAsync(userId,friendID);
            return new Respond<string>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = await result,
                Data = friendID + ""
            };
        }
    }
}
