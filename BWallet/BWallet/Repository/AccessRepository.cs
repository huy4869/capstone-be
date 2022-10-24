using BWallet.DBContexts;
using BWallet.Models;
using BWallet.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using static System.Net.WebRequestMethods;

namespace BWallet.Repository
{
    public class AccessRepository : IAccessRepository
    {
        private readonly MyDBContext context;
        private readonly IConfiguration _configuration;

        public AccessRepository(MyDBContext myDB, IConfiguration configuration)
        {
            this.context = myDB;
            _configuration = configuration;
        }

        public async Task<Account> GetAccountAsync(string phone, string password)
        {
            List<Account> list = await context.Accounts.ToListAsync();
            Account account2 = list.Find(a =>
                a.PhoneNumber.Equals(phone) &&
                 a.Password.Equals(password)
            );
            return account2;
        }

        public async Task<bool> CheckPhoneNumberExistAsync(string phone)
        {
            List<Account> list = await context.Accounts.ToListAsync();
            Account account2 = list.Find(a =>
                a.PhoneNumber.Equals(phone)
            );
            return account2 != null;
        }

        public async Task<string> JWTGenerateAsync(string phone, string? pass)
        {
            //create claims details based on the user information
            var claims = new[] {
                    new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                    new Claim("PhoneNumber", phone),
                    new Claim("Password", pass)
                       };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(_configuration["Jwt:Issuer"], _configuration["Jwt:Audience"],
                claims, expires: DateTime.UtcNow.AddSeconds(60), signingCredentials: signIn);
            var tokenHandler = new JwtSecurityTokenHandler().WriteToken(token);
            return await Task.FromResult(tokenHandler);
        }

        public async Task<bool> CheckOTPAsync(string otp, string enter)
        {
            return await Task.FromResult((otp.Trim().Equals(enter.Trim())));
        }

        public async Task<bool> SendOtpTwilioAsync(string phone, string otp)
        {
            // Find your Account SID and Auth Token at twilio.com/console
            // and set the environment variables. See http://twil.io/secure
            string accountSid = _configuration["Twilio:accountSid"];
            string authToken = _configuration["Twilio:authToken"];

            TwilioClient.Init(accountSid, authToken);
            try
            {
                var message = await MessageResource.CreateAsync(
                               body: "Welcome to B-Wallet, Your OTP is: " + otp,
                               from: new Twilio.Types.PhoneNumber(_configuration["Twilio:from"]),
                               to: new Twilio.Types.PhoneNumber(phone)
                           );
            }
            catch (Exception)
            {

                return await Task.FromResult(false);
            }
            return await Task.FromResult(true);
        }

        public async Task<string> OTPGenerateAsync()
        {
            const string valid = "1234567890";
            int length = 6;
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return await Task.FromResult(res.ToString());
        }

        public async Task RegisterNewUserAsync(string phone, string pass, string name,
           string fb, string bank)
        {
            Account account = new Account();
            User user = new User();
            account.PhoneNumber = phone;
            account.Password = pass;
            await context.Accounts.AddAsync(account);
            context.SaveChanges();
            user.UserName = name;
            user.FBlink = fb;
            user.BankInfo = bank;
            user.AccountID = account.ID;
            await context.Users.AddAsync(user);
            context.SaveChanges();
        }

        public async Task SaveOTPAsync(string phone, string otpCode, string jwt)
        {
            Otp otp = new Otp();
            otp.Phone = phone;
            otp.OtpCode = otpCode;
            otp.CreatedAt = DateTime.Now;
            otp.JWToken = jwt;
            await context.Otps.AddAsync(otp);
            context.SaveChanges();
        }

        public async Task<string> EncryptAsync(string password)
        {
            string key = _configuration["KeyEncrypt"];
            bool useHashing = true;
            byte[] keyArray;
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(password);

            if (useHashing)
            {
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
            }
            else
                keyArray = UTF8Encoding.UTF8.GetBytes(key);

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = keyArray;
            tdes.Mode = CipherMode.ECB;
            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = tdes.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return await Task.FromResult(Convert.ToBase64String(resultArray, 0, resultArray.Length));
        }

        public async Task<string> DecryptAsync(string password)
        {
            string key = _configuration["KeyEncrypt"];
            bool useHashing = true;
            byte[] keyArray;
            byte[] toEncryptArray = Convert.FromBase64String(password);

            if (useHashing)
            {
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
            }
            else
                keyArray = UTF8Encoding.UTF8.GetBytes(key);

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = keyArray;
            tdes.Mode = CipherMode.ECB;
            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = tdes.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return await Task.FromResult(UTF8Encoding.UTF8.GetString(resultArray));

        }

        public async Task ChangePassword(string phone, string newPassword)
        {
            Account account = await context.Accounts.FirstOrDefaultAsync(a =>
                a.PhoneNumber.Equals(phone));
            account.Password = newPassword;
            await context.SaveChangesAsync();
        }

        public async Task<User> GetUserAsync(Account account)
        {
            return await context.Users.FirstOrDefaultAsync(u => u.AccountID == account.ID);
        }

        public async Task<List<User>> GetAllUserAsync()
        {
            return await context.Users.ToListAsync();
        }
    }
}
