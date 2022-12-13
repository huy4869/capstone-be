using G24_BWallet_Backend.Models.ObjectType;
using G24_BWallet_Backend.Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace G24_BWallet_Backend.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class PasswordController : ControllerBase
    {
        private readonly IAccessRepository repo;

        public PasswordController(IAccessRepository repository)
        {
            repo = repository;
        }
        protected int GetUserId()
        {
            return int.Parse(this.User.Claims.First(i => i.Type == "UserId").Value);
        }

        // gửi mã otp
        [HttpPost("send-otp")]
        public async Task<Respond<string>> SendOtp(PhoneParam p)
        {
            var phone = p.Phone.Trim();
            var checkPhone = repo.CheckPhoneNumberExistAsync(phone);
            var otp = repo.OTPGenerateAsync();
            if (await checkPhone == false)
                return new Respond<string>()
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Error = "",
                    Message = "Số điện thoại này chưa được đăng ký!",
                    Data = "Số điện thoại này chưa được đăng ký!"
                };
            if (await repo.SendOtpTwilioAsync(phone, await otp) == false)
                return new Respond<string>()
                {
                    StatusCode = HttpStatusCode.NotAcceptable,
                    Error = "",
                    Message = "Gửi tin nhắn thất bại!",
                    Data = "Gửi tin nhắn thất bại!"
                };
            string jwt = await repo.JWTGenerateAsync(phone, 0);
            await repo.SaveOTPAsync(phone, await otp, jwt);
            return new Respond<string>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Gửi OTP thành công!",
                Data = "Gửi OTP thành công!"
            };
        }

        // kiểm tra opt
        [HttpPost("check-otp")]
        public async Task<Respond<string>> CheckOtp(OtpParam o)
        {
            bool check = await repo.CheckOTPAsync(o.Phone, o.Enter);
            if (check)
                return new Respond<string>()
                {
                    StatusCode = HttpStatusCode.Accepted,
                    Error = "",
                    Message = "Mã OTP đúng!",
                    Data = "Mã OTP đúng!"
                };
            return new Respond<string>()
            {
                StatusCode = HttpStatusCode.NotAcceptable,
                Error = "",
                Message = "Mã OTP sai!",
                Data = "Mã OTP sai!"
            };
        }


        // đổi mật khẩu: return 0 là đổi thành công, 1 là mật khẩu hiện tại sai,
        // 2 là 2 cái mật khẩu mới không trùng nhau
        [HttpPost("change-password")]
        public async Task<Respond<string>> ChangePassword(PasswordChangeParam p)
        {
            int check = await repo.ChangePassword(GetUserId(), p);
            if (check == 1)
                return new Respond<string>()
                {
                    StatusCode = HttpStatusCode.NotAcceptable,
                    Error = "",
                    Message = "Mật khẩu hiện tại sai!",
                    Data = "Mật khẩu hiện tại sai!"
                };
            if (check == 2)
                return new Respond<string>()
                {
                    StatusCode = HttpStatusCode.NotAcceptable,
                    Error = "",
                    Message = "Hai mật khẩu mới không khớp nhau!",
                    Data = "Hai mật khẩu mới không khớp nhau!"
                };
            return new Respond<string>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Đổi mật khẩu thành công!",
                Data = "Đổi mật khẩu thành công!"
            };
        }

    }
}
