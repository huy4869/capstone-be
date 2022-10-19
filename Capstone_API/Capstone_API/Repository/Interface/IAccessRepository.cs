using Capstone_API.Models;
using System.Threading.Tasks;

namespace Capstone_API.Repository.Interface
{
    public interface IAccessRepository
    {
        Task<Account> GetAccountAsync(string phone, string password);
        Task<string> JWTGenerateAsync(string phone, string pass);
        Task<bool> CheckPhoneNumberExistAsync(string phone);
        Task RegisterNewUserAsync(string phone, string pass, string name,
          string fb, string bank);
        Task<bool> SendOtpTwilioAsync(string phone, string otp);
        Task<string> OTPGenerateAsync();
        Task<bool> CheckOTPAsync(string otp, string enter);
        Task SaveOTPAsync(string phone, string otp, string jwt);
        Task<string> EncryptAsync(string password);
        Task<string> DecryptAsync(string password);
        Task ChangePassword(string phone, string newPassword);
        Task<User> GetUserAsync(Account account);
    }
}
