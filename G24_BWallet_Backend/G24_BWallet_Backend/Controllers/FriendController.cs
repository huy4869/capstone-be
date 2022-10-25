using G24_BWallet_Backend.Models;
using G24_BWallet_Backend.Models.ObjectType;
using G24_BWallet_Backend.Repository.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace G24_BWallet_Backend.Controllers
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

        [HttpGet("{userID}")]
        public async Task<Respond<IEnumerable<User>>> GetFriends(int userID)
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
