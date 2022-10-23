using Capstone_API.DBContexts;
using Capstone_API.Repository.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Capstone_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly IAccessRepository accessRepository;
        private readonly IFriendRepository friendRepository;
        private readonly MyDBContext context;

        public TestController(IAccessRepository accessRepository, 
            IFriendRepository friendRepository,MyDBContext context1)
        {
            this.accessRepository = accessRepository;
            this.friendRepository = friendRepository;
            this.context = context1;
        }

        [HttpGet]
        public async Task<IActionResult> Check([FromForm] string phone)
        {
            var check = accessRepository.SendOtpTwilioAsync(phone, "Ko biet");
            return Ok(await check);
        }

        [HttpGet("friend")]
        public IActionResult GetFriend([FromForm] int userID)
        {
            var list = from u in context.Users
                       join f in context.Friends
                       on u.ID equals f.UserID
                       where u.ID == userID
                       select f;
            var list2 = from u in context.Users
                        join l in list
                        on u.ID equals l.UserFriendID
                        select u;
            return Ok( list2 );
        }

        [HttpGet("list")]
        public IActionResult List([FromForm] int userID, [FromForm]List<string> list)
        {
            
            return Ok(list.Count);
        }
    }
}
