using G24_BWallet_Backend.Models;
using G24_BWallet_Backend.Models.ObjectType;
using G24_BWallet_Backend.Repository;
using G24_BWallet_Backend.Repository.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Threading.Tasks;

namespace G24_BWallet_Backend.Controllers
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
        public async Task<Respond<bool>> SendOtp(PhoneParam p)
        {
            var phone = p.Phone;
            bool checkPhoneFormat = await repo.CheckPhoneFormat(phone);
            if (checkPhoneFormat == false)
                return new Respond<bool>()
                {
                    StatusCode = HttpStatusCode.NotAcceptable,
                    Error = "",
                    Message = "Số điện thoại nhập sai định dạng +84....(9 chữ số đằng sau)!",
                    Data = false
                };
            var checkPhone = await repo.CheckPhoneNumberExistAsync(phone);
            var otp = await repo.OTPGenerateAsync();
            if (checkPhone == true)
                return new Respond<bool>()
                {
                    StatusCode = HttpStatusCode.NotAcceptable,
                    Error = "",
                    Message = "Số điện thoại này đã được đăng kí!",
                    Data = false
                };
            if (await repo.SendOtpTwilioAsync(phone, otp) == false)
                return new Respond<bool>()
                {
                    StatusCode = HttpStatusCode.NotAcceptable,
                    Error = "",
                    Message = "Gửi otp thất bại (có thể là do nhập sai số điện thoại)",
                    Data = false
                };
            string jwt = await repo.JWTGenerateAsync(phone, 0);
            await repo.SaveOTPAsync(phone, otp, jwt);
            return new Respond<bool>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Gửi otp thành công",
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
        public async Task<Respond<bool>> CheckOtp(OtpParam o)
        {
            // check thời gian 1 otp là 5 phút
            bool timeCheck = await repo.CheckOTPTimeAsync(o.Phone,5);
            if (timeCheck == false)
                return new Respond<bool>()
                {
                    StatusCode = HttpStatusCode.RequestTimeout,// 408
                    Error = "",
                    Message = "Mã OTP đã hết hạn!",
                    Data = false
                };
            bool check = await repo.CheckOTPAsync(o.Phone, o.Enter);
            if(check)
                return new Respond<bool>()
                {
                    StatusCode = HttpStatusCode.Accepted,
                    Error = "",
                    Message = "Mã OTP chính xác!",
                    Data = true
                };
            return new Respond<bool>()
            {
                StatusCode = HttpStatusCode.NotAcceptable,
                Error = "",
                Message = "Mã OTP sai!",
                Data = false
            };
        }

        [HttpPost("register")]
        public async Task<Respond<bool>> SignUp(RegisterParam r)
        {
            await repo.RegisterNewUserAsync(r.Phone,await repo.EncryptAsync(r.Password), r.Name, null, null);
            return new Respond<bool>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Đăng ký thành công!",
                Data = true
            };
        }
    }
}
