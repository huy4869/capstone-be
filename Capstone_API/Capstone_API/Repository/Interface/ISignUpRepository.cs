namespace Capstone_API.Repository.Interface
{
    public interface ISignUpRepository
    {
        public void RegisterNewUser(string phone, string pass, string name,
            string fb, string bank);
        public bool SendOtpTwilio(string phone, string otp);
        public string OTPGenerate();
        public bool CheckOTP(string otp, string enter);
        public void SaveOTP(string phone, string otp, string jwt);
    }
}
