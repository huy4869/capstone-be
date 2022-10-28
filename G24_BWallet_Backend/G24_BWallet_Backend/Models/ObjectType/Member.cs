namespace G24_BWallet_Backend.Models.ObjectType
{
    public class Member
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserAvatar { get; set; }
        public string UserPhone { get; set; }

        public Member()
        {
        }

        public Member(int userId, string userName, string userAvatar, string userPhone)
        {
            UserId = userId;
            UserName = userName;
            UserAvatar = userAvatar;
            UserPhone = userPhone;
        }
    }
}
