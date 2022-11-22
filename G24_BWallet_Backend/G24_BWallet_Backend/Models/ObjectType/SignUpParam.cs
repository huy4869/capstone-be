namespace G24_BWallet_Backend.Models.ObjectType
{
    public class SignUpParam
    {
    }

    public class PhoneParam
    {
        public string Phone { get; set; }
    }

    public class OtpParam
    {
        public string Otp { get; set; }
        public string Enter { get; set; }
    }

    public class RegisterParam
    {
        public string Phone { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
    }

    public class PasswordChangeParam
    {
        public string Phone { get; set; }
        public string Password { get; set; }
    }
}
