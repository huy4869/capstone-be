using Capstone_API.Models;
using Capstone_API.Repository.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Capstone_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SignUpController : ControllerBase
    {
        private readonly ISignUpRepository _repo;
        private readonly IManagerRepository _mrepo;

        public SignUpController(ISignUpRepository repo, IManagerRepository mrepo)
        {
            _repo = repo;
            _mrepo = mrepo;
        }

        [HttpPost("send-otp")]
        public IActionResult SendOtp([FromForm] string phone)
        {
            if (_mrepo.CheckPhoneNumberExist(phone) == true)
                return NotFound("This phone number is registed!");
            string otp = _repo.OTPGenerate();
            if (_repo.SendOtpTwilio(phone,otp) == false)
                return NotFound("This phone number is not exist!");
            string jwt = _mrepo.JWTGenerate(phone,"");
            _repo.SaveOTP(phone,otp,jwt);
            return Ok("Sent");
        }

        [HttpPost("check-otp")]
        public IActionResult CheckOtp([FromForm] string otp, [FromForm] string enter)
        {
            bool check = _repo.CheckOTP(otp, enter);
            return check ? Ok("OTP is correct!") : NotFound("OTP is wrong!");
        }

        [HttpPost("register")]
        public IActionResult SignUp([FromForm] string phone, [FromForm] string password,
            [FromForm] string name, [FromForm] string fb, [FromForm] string bank)
        {
            _repo.RegisterNewUser(phone, password, name, fb, bank);
            return Ok("Register success!!");
        }
    }
}
