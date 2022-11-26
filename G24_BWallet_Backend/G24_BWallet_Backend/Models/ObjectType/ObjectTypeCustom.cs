using System;
using System.Collections.Generic;

namespace G24_BWallet_Backend.Models.ObjectType
{
    public class ObjectTypeCustom
    {
    }

    public class EventHome
    {
        public int EventId { get; set; }
        public string EventName { get; set; }
        public string EventLogo { get; set; }
        public double TotalMoney { get; set; }
        public List<UserHome> ListUser { get; set; }
    }

    public class UserHome
    {
        public string UserName { get; set; }
        public string Avatar { get; set; }
        public double Money { get; set; }
        public string MoneyColor { get; set; }
    }

    public class EventUserID
    {
        public int EventId { get; set; }
        public int UserId { get; set; }
    }

    public class JwtParam
    {
        public string access_token { get; set; }
        public string type_token { get; set; }
    }

    public class NewEvent
    {
        public string EventName { get; set; }
        public string EventLogo { get; set; }
        public string EventDescript { get; set; }
        public List<int> MemberIDs { get; set; }
    }

    public class PaidDebtParam
    {
        public int UserId { get; set; }
        public int EventId { get; set; }
        public string PaidImage { get; set; }
        public double TotalMoney { get; set; }
        public List<RenamePaidDebtList> ListEachPaidDebt { get; set; }
        public List<string> IMGLinks { get; set; }
    }

    public class RenamePaidDebtList
    {
        public int userDeptId { get; set; }
        public int debtLeft { get; set; }
    }

    public class PhoneParam
    {
        public string Phone { get; set; }
    }

    public class OtpParam
    {
        public string Phone { get; set; }
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

    public class UserAvatarName
    {
        public string Avatar { get; set; }
        public string Name { get; set; }
    }

    public class UserDebtReturn
    {
        public int UserDeptId { get; set; }
        public string ReceiptName { get; set; }
        public string Date { get; set; }
        public string OwnerName { get; set; }
        public double DebtLeft { get; set; }
    }

    public class UserPhone
    {
        public User User { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class RequestJoinParam
    {
        public int EventId { get; set; }
        public string EventLogo { get; set; }
        public string EventName { get; set; }
        public DateTime Date { get; set; }
        public int Status { get; set; }
    }

    public class InviteJoinParam
    {
        public int InviteId { get; set; }
        public string UserName { get; set; }
        public string EventLogo { get; set; }
        public string EventName { get; set; }
        public DateTime Date { get; set; }
    }

    public class InviteRespondParam
    {
        public int InviteId { get; set; }
        public int Status { get; set; }
    }

    public class EventFriendParam
    {
        public int EventId { get; set; }
        public int UserId { get; set; }
        public List<int> MemberIDs { get; set; }
    }

    public class UserJoinRequestParam
    {
        public int UserId { get; set; }
        public string Avatar { get; set; }
        public string UserName { get; set; }
        public string Phone { get; set; }
        public int Status { get; set; }

    }

    public class IdAvatarNameRole
    {
        public int UserId { get; set; }
        public string Avatar { get; set; }
        public string Name { get; set; }
        public int Role { get; set; }
    }
    public class IdAvatarNamePhone
    {
        public int UserId { get; set; }
        public string Avatar { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
    }
    public class MemberDetailParam
    {
        public string EventName { get; set; }
        public IdAvatarNameRole Inspector { get; set; }
        public IdAvatarNameRole Cashier { get; set; }
        public List<IdAvatarNamePhone> Members { get; set; }
    }

    public class EventIdNameDes
    {
        public int EventId { get; set; }
        public string EventName { get; set; }
        public string EventDescript { get; set; }
    }
}
