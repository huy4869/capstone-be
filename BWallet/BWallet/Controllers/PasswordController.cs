using BWallet.Models.ObjectType;
using BWallet.Repository.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;

namespace BWallet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PasswordController : ControllerBase
    {
        private readonly IAccessRepository repo;

        public PasswordController(IAccessRepository repository)
        {
            repo = repository;
        }

        [HttpPost("send-otp")]
        public async Task<Respond<bool>> SendOtp([FromForm] string phone)
        {
            var checkPhone = repo.CheckPhoneNumberExistAsync(phone);
            var otp = repo.OTPGenerateAsync();
            if (await checkPhone == false)
                return new Respond<bool>()
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Error = "",
                    Message = "This phone number is not registed!",
                    Data = false
                };
            if (await repo.SendOtpTwilioAsync(phone, await otp) == false)
                return new Respond<bool>()
                {
                    StatusCode = HttpStatusCode.NotAcceptable,
                    Error = "",
                    Message = "This phone number is not exist!",
                    Data = false
                };
            string jwt = await repo.JWTGenerateAsync(phone, "");
            await repo.SaveOTPAsync(phone, await otp, jwt);
            return new Respond<bool>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Send OTP success!",
                Data = true
            };
        }

        [HttpPost("check-otp")]
        public async Task<Respond<bool>> CheckOtp([FromForm] string otp, [FromForm] string enter)
        {
            bool check = await repo.CheckOTPAsync(otp, enter);
            if (check)
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

        [HttpPost("change-password")]
        public async Task<Respond<string>> ChangePassword([FromForm] string phone, [FromForm] string newPassword)
        {
            await repo.ChangePassword(phone, newPassword);
            return new Respond<string>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "",
                Data = "Change password success!"
            };
        }

    }
}
