using G24_BWallet_Backend.DBContexts;
using G24_BWallet_Backend.Models;
using G24_BWallet_Backend.Models.ObjectType;
using G24_BWallet_Backend.Repository.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twilio.Rest.Chat.V2.Service.User;

namespace G24_BWallet_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly IAccessRepository accessRepository;
        private readonly IFriendRepository friendRepository;
        private readonly MyDBContext context;

        public TestController(IAccessRepository accessRepository,
            IFriendRepository friendRepository, MyDBContext context1)
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
            return Ok(list2);
        }

        [HttpGet("list")]
        public IActionResult List([FromForm] int userID, [FromForm] List<string> list)
        {

            return Ok(list.Count);
        }

        [HttpGet("testint")]
        public IActionResult List()
        {
            Event e = new Event();
            List<int> list = new List<int> { 1, 2, 3, 22, 9999, 213 };
            NewEvent ne = new NewEvent();
            ne.Event = e;
            ne.MemberIDs = list;
            return Ok(ne);
        }

        [HttpGet("ListFriend")]
        public IActionResult FriendPhone()
        {
            var list = from u in context.Users
                       join f in context.Friends
                       on u.ID equals f.UserID
                       where u.ID == 4
                       select f;
            var list2 = from u in context.Users.Include(u => u.Account)
                        join l in list
                        on u.ID equals l.UserFriendID
                        select (new {u.ID,u.Avatar,u.UserName,u.Account.PhoneNumber} );
            //var list3 = new List<UserPhone>();
            //foreach (var item in list2)
            //{
            //    UserPhone up = new UserPhone();
            //    up.User = item;
            //    up.PhoneNumber = item.Account.PhoneNumber;
            //    list3.Add(up);
            //}
            return Ok(list2);
        }

        [HttpGet("phone")]
        public IActionResult GetPhoneByUserID()
        {
            var userid = 4;
            string phone = context.Users.Include(x => x.Account).FirstOrDefault(u=> u.ID == userid).Account.PhoneNumber;
            //var listFriend = context.Users.
            return Ok(phone);
        }
    }
}
