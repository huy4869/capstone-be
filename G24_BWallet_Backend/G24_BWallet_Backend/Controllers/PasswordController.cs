using G24_BWallet_Backend.Models.ObjectType;
using G24_BWallet_Backend.Repository.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;

namespace G24_BWallet_Backend.Controllers
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
        public async Task<Respond<bool>> SendOtp(PhoneParam p)
        {
            var phone = p.Phone.Trim();
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
            string jwt = await repo.JWTGenerateAsync(phone, 0);
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
        public async Task<Respond<bool>> CheckOtp(OtpParam o)
        {
            bool check = await repo.CheckOTPAsync(o.Otp, o.Enter);
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
        public async Task<Respond<string>> ChangePassword(PasswordChangeParam p)
        {
            await repo.ChangePassword(p.Phone, await repo.EncryptAsync(p.Password));
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
