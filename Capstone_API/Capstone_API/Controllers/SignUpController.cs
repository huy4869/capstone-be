using Capstone_API.Models;
using Capstone_API.Models.ObjectType;
using Capstone_API.Repository;
using Capstone_API.Repository.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Capstone_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // handle sign up a new user 
    public class SignUpController : ControllerBase
    {
        private readonly IAccessRepository repo;

        public SignUpController(IAccessRepository repo)
        {
            this.repo = repo;
        }

        [HttpPost("send-otp")]
        public async Task<Respond<bool>> SendOtp([FromForm] string phone)
        {
            string phone2 = phone;
            var checkPhone = await repo.CheckPhoneNumberExistAsync(phone);
            var otp = await repo.OTPGenerateAsync();
            if (checkPhone == true)
                return new Respond<bool>()
                {
                    StatusCode = HttpStatusCode.NotAcceptable,
                    Error = "",
                    Message = "This phone number is registed!",
                    Data = false
                };
            if (await repo.SendOtpTwilioAsync(phone, otp) == false)
                return new Respond<bool>()
                {
                    StatusCode = HttpStatusCode.NotAcceptable,
                    Error = "",
                    Message = "This phone number is not exist!",
                    Data = false
                };
            string jwt = await repo.JWTGenerateAsync(phone, "");
            await repo.SaveOTPAsync(phone, otp, jwt);
            return new Respond<bool>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Send OTP success!",
                Data = true
            };
        }
        //[HttpPost("send-otp")]
        //public IActionResult SendOtp([FromForm] string phone)
        //{
        //    var checkPhone = repo.CheckPhoneNumberExistAsync(phone);
        //    var otp = repo.OTPGenerateAsync();
        //    //if (await checkPhone == true)
        //    //    return new Respond<bool>()
        //    //    {
        //    //        StatusCode = HttpStatusCode.NotAcceptable,
        //    //        Error = "",
        //    //        Message = "This phone number is registed!",
        //    //        Data = false
        //    //    };
        //    //if (await repo.SendOtpTwilioAsync(phone, await otp) == false)
        //    //    return new Respond<bool>()
        //    //    {
        //    //        StatusCode = HttpStatusCode.NotAcceptable,
        //    //        Error = "",
        //    //        Message = "This phone number is not exist!",
        //    //        Data = false
        //    //    };
        //    //string jwt = await repo.JWTGenerateAsync(phone, "");
        //    //await repo.SaveOTPAsync(phone, await otp, jwt);
        //    //return new Respond<bool>()
        //    //{
        //    //    StatusCode = HttpStatusCode.Accepted,
        //    //    Error = "",
        //    //    Message = "Send OTP success!",
        //    //    Data = true
        //    //};
        //}
        [HttpPost("check-otp")]
        public async Task<Respond<bool>> CheckOtp([FromForm] string otp, [FromForm] string enter)
        {
            bool check = await repo.CheckOTPAsync(otp, enter);
            if(check)
                return new Respond<bool>()
                {
                    StatusCode = HttpStatusCode.Accepted,
                    Error = "",
                    Message = "OTP is correct!",
                    Data = true
                };
            return new Respond<bool>()
            {
                StatusCode = HttpStatusCode.NotAcceptable,
                Error = "",
                Message = "OTP is wrong!",
                Data = false
            };
        }

        [HttpPost("register")]
        public async Task<Respond<bool>> SignUp([FromForm] string phone, [FromForm] string password,
            [FromForm] string name, [FromForm] string fb, [FromForm] string bank)
        {
            await repo.RegisterNewUserAsync(phone,await repo.EncryptAsync(password), name, fb, bank);
            return new Respond<bool>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Register success!",
                Data = true
            };
        }
    }
}
