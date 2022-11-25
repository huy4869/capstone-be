using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using G24_BWallet_Backend.DBContexts;
using G24_BWallet_Backend.Models;
using G24_BWallet_Backend.Models.ObjectType;
using G24_BWallet_Backend.Repository.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Twilio.Http;
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
        private readonly IConfiguration _configuration;

        public TestController(IAccessRepository accessRepository,
            IFriendRepository friendRepository, MyDBContext context1,
            IConfiguration _configuration)
        {
            this.accessRepository = accessRepository;
            this.friendRepository = friendRepository;
            this.context = context1;
            this._configuration = _configuration;
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

        //[HttpGet("testint")]
        //public IActionResult List()
        //{
        //    Event e = new Event();
        //    List<int> list = new List<int> { 1, 2, 3, 22, 9999, 213 };
        //    NewEvent ne = new NewEvent();
        //    ne.Event = e;
        //    ne.MemberIDs = list;
        //    return Ok(ne);
        //}

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
                        select (new { u.ID, u.Avatar, u.UserName, u.Account.PhoneNumber });
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
            string phone = context.Users.Include(x => x.Account).FirstOrDefault(u => u.ID == userid).Account.PhoneNumber;
            //var listFriend = context.Users.
            return Ok(phone);
        }

        [HttpGet("dic")]
        public IActionResult Dictionary()
        {

            var s1 = new { StudentName = "duy", Age = 22 };
            var s2 = new { StudentName = "quang", Age = 18 };
            var list = new Dictionary<Object, Object>()
            {
                {nameof(s1.StudentName),s1.StudentName },
                {nameof(s1.Age),s1.Age }
            };
            return Ok(list);
        }

        [HttpGet("testparam")]
        public IActionResult TestParamPass(int age, string name)
        {
            string a = "Hello " + name + " Age: " + age;
            return Ok(a);
        }

        [HttpPost("testimg")]
        public async Task<string> TestIMG([FromForm] IFormFile imgFile, [FromForm] Receipt receipt)
        {
            /*string AWSS3AccessKeyId = _configuration["AWSS3:AccessKeyId"];
            string AWSS3SecretAccessKey = _configuration["AWSS3:SecretAccessKey"];
            using (var client = new AmazonS3Client(AWSS3AccessKeyId, AWSS3SecretAccessKey, RegionEndpoint.APSoutheast1))
            {
                using (var newMemoryStream = new MemoryStream())
                {
                    imgFile.CopyTo(newMemoryStream);

                    var uploadRequest = new TransferUtilityUploadRequest
                    {
                        InputStream = newMemoryStream,
                        Key = imgFile.FileName,
                        BucketName = "bwallets3bucket/users"
                    };

                    var fileTransferUtility = new TransferUtility(client);
                    await fileTransferUtility.UploadAsync(uploadRequest);
                }
            }*/
            return "receipt name: " + receipt.ReceiptName + "//" + receipt.User.UserName + " is who make it";
        }

        [HttpPost("testTimeZone")]
        public async Task<Object> TestTimeZone()//SE Asia Standard Time
        {
            //var list = TimeZoneInfo.GetSystemTimeZones();
            var VietNamTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
            return new { VietNamTime };
        }
    }
}
