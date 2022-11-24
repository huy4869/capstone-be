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

        [HttpGet]
        public async Task<Respond<IEnumerable<Member>>> GetFriends()
        {
            int userId = GetUserId();
            var friends = repo.GetFriendsAsync(GetUserId());
            return new Respond<IEnumerable<Member>>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Get friends success",
                Data = await friends
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
                Message = "Đã mời bạn bè vào event này, chờ họ accept",
                Data = null
            };
        }
    }
}
