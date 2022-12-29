using Amazon.Runtime.Internal.Util;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon.S3;
using G24_BWallet_Backend.DBContexts;
using G24_BWallet_Backend.Models;
using G24_BWallet_Backend.Models.ObjectType;
using G24_BWallet_Backend.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using static System.Net.WebRequestMethods;
using Amazon;
using System.Web;

namespace G24_BWallet_Backend.Repository
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

        public async Task<string> JWTGenerateAsync(string phone, int userId)
        {
            //create claims details based on the user information
            var claims = new[] {
                    new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                    new Claim("PhoneNumber", phone),
                    new Claim("UserId",userId.ToString())
                       };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(_configuration["Jwt:Issuer"], _configuration["Jwt:Audience"],
                claims, expires: DateTime.UtcNow.AddMonths(6), signingCredentials: signIn);
            var tokenHandler = new JwtSecurityTokenHandler().WriteToken(token);
            return await Task.FromResult(tokenHandler);
        }

        public async Task<bool> CheckOTPAsync(string phone, string enter)
        {
            var otp = await context.Otps.OrderBy(o => o.OtpID)
                .LastOrDefaultAsync(o => o.Phone.Equals(phone.Trim()));
            return await Task.FromResult((otp.OtpCode.Trim().Equals(enter.Trim())));
        }

        // kiểm tra otp còn hạn hay ko
        public async Task<bool> CheckOTPTimeAsync(string phone, int second)
        {
            var otp = await context.Otps.OrderBy(o => o.OtpID)
                 .LastOrDefaultAsync(o => o.Phone.Equals(phone.Trim()));
            DateTime createTime = otp.CreatedAt;
            DateTime now = DateTime.Now;
            TimeSpan diffResult = now.Subtract(createTime);
            if (diffResult.TotalSeconds > second)
                return false;
            return true;
        }

        // gửi tin nhắn về số điện thoại
        public async Task<bool> SendOtpTwilioAsync(string phone, string otp)
        {
            // Find your Account SID and Auth Token at twilio.com/console
            // and set the environment variables. See http://twil.io/secure
            string accountSid = _configuration["Twilio:accountSid"];
            string authToken = _configuration["Twilio:authToken"];
            string apiKey = _configuration["Twilio:ApiKeySid"];
            string apiSecret = _configuration["Twilio:ApiKeySecret"];

            //TwilioClient.Init(accountSid, authToken);
            TwilioClient.Init(apiKey, apiSecret, accountSid);
            try
            {
                var message = await MessageResource.CreateAsync(
                               body: "Mã OTP của bạn là: " + otp+". Mã chỉ có hiệu lực trong vòng 5 phút, " +
                               "vui lòng truy cập B-Wallet để tiếp tục!",
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
            user.AllowAddFriendStatus = 1;
            user.AllowInviteEventStatus = 1;
            user.AccountID = account.ID;
            await context.Users.AddAsync(user);
            context.SaveChanges();
        }

        // lưu otp vào db
        public async Task SaveOTPAsync(string phone, string otpCode, string jwt)
        {
            DateTime VNDateTime = TimeZoneInfo.ConvertTime(DateTime.Now,
                TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
            Otp otp = new Otp();
            otp.Phone = phone;
            otp.OtpCode = otpCode;
            otp.CreatedAt = VNDateTime;
            otp.JWToken = jwt;
            await context.Otps.AddAsync(otp);
            context.SaveChanges();
        }

        public async Task<string> EncryptAsync(string password)
        {
            string key = _configuration["KeyEncrypt"];
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(password);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);
        }

        public async Task<string> DecryptAsync(string password)
        {
            string key = _configuration["KeyEncrypt"];
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(password);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }

        }

        // đổi mật khẩu: return 0 là đổi thành công, 1 là mật khẩu hiện tại sai,
        // 2 là 2 cái mật khẩu mới không trùng nhau
        public async Task<int> ChangePassword(int userId, PasswordChangeParam p)
        {
            p.password = await EncryptAsync(p.password);
            User user = await context.Users.Include(u => u.Account)
                .FirstOrDefaultAsync(u => u.ID == userId);
            if (!user.Account.Password.Equals(p.password))
                return 1;
            if (!p.new_password.Equals(p.password_confirmation))
                return 2;
            user.Account.Password = await EncryptAsync(p.new_password);
            await context.SaveChangesAsync();
            return 0;
        }

        public async Task<User> GetUserAsync(Account account)
        {
            return await context.Users.FirstOrDefaultAsync(u => u.AccountID == account.ID);
        }

        public async Task<List<User>> GetAllUserAsync()
        {
            return await context.Users.ToListAsync();
        }

        public Task<bool> CheckPhoneFormat(string phone)
        {
            var regex = new Regex("^\\+84[0-9]{9,10}$");
            return Task.FromResult(regex.IsMatch(phone.Trim()));
        }

        public async Task UpdateUserProfile(User userEditInfo, int userId)
        {
            User user = await context.Users.FirstOrDefaultAsync(u => u.ID == userId);
            if (!userEditInfo.Avatar.IsNullOrEmpty())
            {
                await DeleteS3FileByLink(user.Avatar);
                user.Avatar = userEditInfo.Avatar.Trim();
            }

            if (!userEditInfo.UserName.IsNullOrEmpty())
            {
                userEditInfo.UserName = Regex.Replace(userEditInfo.UserName, @"\s+", " ");
                user.UserName = userEditInfo.UserName.Trim();
            }

            user.AllowAddFriendStatus = userEditInfo.AllowAddFriendStatus;
            user.AllowInviteEventStatus = userEditInfo.AllowInviteEventStatus;
            await context.SaveChangesAsync();
        }
        public async Task DeleteS3FileByLink(string link = null)
        {
            if(link == null)
            {
                return;
            }
            Format f = new Format();
            string AWSS3AccessKeyId = await f.DecryptAsync(_configuration["AWSS3:DeleteKey"]);
            string AWSS3SecretAccessKey = await f.DecryptAsync(_configuration["AWSS3:DeleteSecretKey"]);
            var client = new AmazonS3Client(AWSS3AccessKeyId, AWSS3SecretAccessKey, RegionEndpoint.APSoutheast1);

            var respone = await client.DeleteObjectAsync(new Amazon.S3.Model.DeleteObjectRequest()
            {
                BucketName = "bwallets3bucket/user",
                Key = HttpUtility.UrlDecode(link.Split('/').Last())
            });
            var isdelete = respone.DeleteMarker;
        }

        // đặt lại mật khẩu khi quên
        public async Task<int> NewPassword(NewPassword p)
        {
            Account account = await context.Accounts
                .FirstOrDefaultAsync(a => a.PhoneNumber.Equals(p.phone));
            if (p.password.Equals(p.password_confirmation))
            {
                account.Password = await EncryptAsync(p.password_confirmation);
                await context.SaveChangesAsync();
                return 0;
            }
            return 1;
        }
    }
}
