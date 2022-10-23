using Capstone_API.Models;
using Capstone_API.Models.ObjectType;
using Capstone_API.Repository.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Capstone_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FriendController : ControllerBase
    {
        private readonly IFriendRepository repo;

        public FriendController(IFriendRepository friendRepository)
        {
            repo = friendRepository;
        }

        [HttpGet]
        public async Task<Respond<IEnumerable<User>>> GetFriends([FromForm] int userID)
        {
            var friends = repo.GetFriendsAsync(userID);
            return new Respond<IEnumerable<User>>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Get friends success",
                Data = await friends
            };
        }
    }
}
