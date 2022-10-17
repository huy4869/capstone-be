using Capstone_API.Models;
using System.Threading.Tasks;

namespace Capstone_API.Repository.Interface
{
    public interface IAccessRepository
    {
        public Task<Account> GetAccountAsync(string phone, string password);
        public Task<string> JWTGenerateAsync(string phone, string pass);
        public Task<bool> CheckPhoneNumberExistAsync(string phone);
        public Task RegisterNewUserAsync(string phone, string pass, string name,
           string fb, string bank);
        public Task<bool> SendOtpTwilioAsync(string phone, string otp);
        public Task<string> OTPGenerateAsync();
        public Task<bool> CheckOTPAsync(string otp, string enter);
        public Task SaveOTPAsync(string phone, string otp, string jwt);
        public Task<string> EncryptAsync(string password);
        public Task<string> DecryptAsync(string password);
    }
}
