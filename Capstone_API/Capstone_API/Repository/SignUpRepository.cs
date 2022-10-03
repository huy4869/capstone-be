using Capstone_API.DBContexts;
using Capstone_API.Models;
using Capstone_API.Repository.Interface;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using System;
using System.Text;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace Capstone_API.Repository
{
    public class SignUpRepository : ISignUpRepository
    {
        private readonly MyDBContext _myDB;
        private readonly IConfiguration _configuration;

        public SignUpRepository(MyDBContext myDB, IConfiguration configuration)
        {
            _myDB = myDB;
            _configuration = configuration;
        }

        public bool CheckOTP(string otp, string enter)
        {
           return (otp.Trim().Equals(enter.Trim())) ? true : false;
        }

        public bool SendOtpTwilio(string phone, string otp)
        {
            // Find your Account SID and Auth Token at twilio.com/console
            // and set the environment variables. See http://twil.io/secure
            string accountSid = _configuration["Twilio:accountSid"];
            string authToken = _configuration["Twilio:authToken"];

            TwilioClient.Init(accountSid, authToken);
            try
            {
                var message = MessageResource.Create(
                               body: "Welcome to B-Wallet, Your OTP is: "+ otp,
                               from: new Twilio.Types.PhoneNumber(_configuration["Twilio:from"]),
                               to: new Twilio.Types.PhoneNumber(phone)
                           );
            }
            catch (Exception)
            {

                return false;
            }
            return true;
        }

        public string OTPGenerate()
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEF" +
                "GHIJKLMNOPQRSTUVWXYZ1234567890";
            int length = 6;
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }

        public void RegisterNewUser(string phone, string pass, string name, 
            string fb, string bank)
        {
            Account account = new Account();
            User user = new User();
            account.PhoneNumber = phone;
            account.Password = pass;
            _myDB.Accounts.Add(account);
            _myDB.SaveChanges();
            user.UserName = name;
            user.FBlink = fb;
            user.BankInfo = bank;
            user.Account = account;
            _myDB.Users.Add(user);
            _myDB.SaveChanges();
        }

        public void SaveOTP(string phone, string otpCode,string jwt)
        {
            Otp otp = new Otp();
            otp.Phone = phone;
            otp.OtpCode = otpCode;
            otp.CreatedAt = DateTime.Now;
            otp.JWToken = jwt;
            _myDB.Otps.Add(otp);
            _myDB.SaveChanges();
        }
    }
}
