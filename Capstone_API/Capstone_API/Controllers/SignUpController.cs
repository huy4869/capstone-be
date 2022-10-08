using Capstone_API.Models;
using Capstone_API.Repository.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;

namespace Capstone_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // handle sign up a new user 
    public class SignUpController : ControllerBase
    {
        private readonly IAccessRepository _repo;

        public SignUpController(IAccessRepository repo)
        {
            _repo = repo;
        }

        [HttpPost("send-otp")]
        public Respond<bool> SendOtp([FromForm] string phone)
        {
            if (_repo.CheckPhoneNumberExist(phone) == true)
            return new Respond<bool>()
            {
                StatusCode = HttpStatusCode.NotAcceptable,
                Error = "",
                Message = "This phone number is registed!",
                Data = false
            };
            string otp = _repo.OTPGenerate();
            if (_repo.SendOtpTwilio(phone,otp) == false)
            return new Respond<bool>()
            {
                StatusCode = HttpStatusCode.NotAcceptable,
                Error = "",
                Message = "This phone number is not exist!",
                Data = false
            };
            string jwt = _repo.JWTGenerate(phone,"");
            _repo.SaveOTP(phone,otp,jwt);
            return new Respond<bool>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Send OTP success!",
                Data = true
            };
        }

        [HttpPost("check-otp")]
        public Respond<bool> CheckOtp([FromForm] string otp, [FromForm] string enter)
        {
            bool check = _repo.CheckOTP(otp, enter);
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
        public Respond<bool> SignUp([FromForm] string phone, [FromForm] string password,
            [FromForm] string name, [FromForm] string fb, [FromForm] string bank)
        {
            _repo.RegisterNewUser(phone,_repo.Encrypt(password), name, fb, bank);
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
