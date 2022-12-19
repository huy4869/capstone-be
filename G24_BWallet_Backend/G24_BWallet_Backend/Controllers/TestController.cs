using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using G24_BWallet_Backend.DBContexts;
using G24_BWallet_Backend.Models;
using G24_BWallet_Backend.Models.ObjectType;
using G24_BWallet_Backend.Repository;
using G24_BWallet_Backend.Repository.Interface;
using Microsoft.AspNetCore.Authorization;
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
using Twilio;
using Twilio.Http;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Rest.Chat.V2.Service.User;
using Twilio.TwiML.Voice;
using static System.Net.WebRequestMethods;

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

        [HttpGet("testTimeZone")]
        public async Task<Object> TestTimeZone([FromQuery] double money)//SE Asia Standard Time
        {
            //var list = TimeZoneInfo.GetSystemTimeZones();
            //var VietNamTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

            //double money = 12345689.324;
            /*Format f = new Format();
            //string formatmoney = f.MoneyFormat(money);
            string formatmoney = money.ToString("C10");
            money = (int)(money / 1);*/
            Format f = new Format();
            //return await f.EncryptAsync("ZGvCyihrgbk9SLnQBneji11YhULzqLCXB4md5jNZ");
            return null;
        }

        class Product
        {
            public string Name { get; set; }
            public double Money { get; set; }
            public string MoneyFormat { get; set; }
        }

        [HttpGet("moneyFormat")]
        public IActionResult MoneyFormat()
        {
            Format f = new Format();
            Product p1 = new Product { Name = "Laptop", Money = 100.76 };
            Product p2 = new Product { Name = "IPhone", Money = 1000 };
            Product p3 = new Product { Name = "IPhone", Money = 10000 };
            Product p4 = new Product { Name = "IPhone", Money = 100000 };
            Product p5 = new Product { Name = "IPhone", Money = 1000000 };
            Product p6 = new Product { Name = "IPhone", Money = 10000000 };
            Product p7 = new Product { Name = "IPhone", Money = 100000000 };
            Product p8 = new Product { Name = "IPhone", Money = 1000000000 };
            Product p9 = new Product { Name = "IPhone", Money = 10000000000 };
            Product p10 = new Product { Name = "IPhone", Money = 100000000000 };
            Product p11 = new Product { Name = "IPhone", Money = 1000000000000 };
            p1.MoneyFormat = f.MoneyFormat(p1.Money);
            p2.MoneyFormat = f.MoneyFormat(p2.Money);
            p3.MoneyFormat = f.MoneyFormat(p3.Money);
            p4.MoneyFormat = f.MoneyFormat(p4.Money);
            p5.MoneyFormat = f.MoneyFormat(p5.Money);
            p6.MoneyFormat = f.MoneyFormat(p6.Money);
            p7.MoneyFormat = f.MoneyFormat(p7.Money);
            p8.MoneyFormat = f.MoneyFormat(p8.Money);
            p9.MoneyFormat = f.MoneyFormat(p9.Money);
            p10.MoneyFormat = f.MoneyFormat(p10.Money);
            p11.MoneyFormat = f.MoneyFormat(p11.Money);
            return Ok(new { p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11 });
        }

        protected int GetUserId()
        {
            return int.Parse(this.User.Claims.First(i => i.Type == "UserId").Value);
        }
        [HttpGet("token")]
        public IActionResult GetToken()
        {
            return Ok(GetUserId());
        }

        [HttpGet("encrypt")]
        public async Task<IActionResult> GetEncrypt()
        {
            Format format = new Format();
            var a = await format.EncryptAsync("123");
            var b = await format.DecryptAsync(a);
            return Ok($"a:{a}   b:{b}");
        }

        [HttpGet("sms")]
        public async Task<IActionResult> SendSms([FromQuery] string phone, [FromQuery] string content)
        {
            // Find your Account SID and Auth Token at twilio.com/console
            // and set the environment variables. See http://twil.io/secure
            string accountSid = _configuration["Twilio:accountSid"];
            string authToken = _configuration["Twilio:authToken"];
            string apiKey = _configuration["Twilio:ApiKeySid"];
            string apiSecret = _configuration["Twilio:ApiKeySecret"];

            //TwilioClient.Init(accountSid, authToken);
            TwilioClient.Init(apiKey, apiSecret,accountSid);
            try
            {
                var message = await MessageResource.CreateAsync(
                               body: "Twilio test: " + content,
                               from: new Twilio.Types.PhoneNumber(_configuration["Twilio:from"]),
                               to: new Twilio.Types.PhoneNumber("+" + phone)
                           );
            }
            catch (Exception ex)
            {

                return Ok("Exception: " + ex.Message + "." +
                    "\nNếu không nhận được tin nhắn thì kiểm tra 2 thứ: 1 là số điện thoại," +
                    " 2 là mã authen trên twilio đã bị thay đổi(fix trong file appsetting.json)!");
            }
            return Ok("Nếu không nhận được tin nhắn thì kiểm tra 2 thứ: 1 là số điện thoại phải có" +
                " +84...., 2 là mã authen trên twilio đã bị thay đổi!");
        }
    }
}
