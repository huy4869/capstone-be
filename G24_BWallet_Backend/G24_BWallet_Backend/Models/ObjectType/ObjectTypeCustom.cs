using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

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
        public int EventStatus { get; set; }
        public NumberMoney Debt { get; set; }
        public NumberMoney Receive { get; set; }
        public MoneyColor TotalMoney { get; set; }
        public int ReceiptCount { get; set; }
    }

    public class NumberMoney
    {
        public int TotalPeople { get; set; }
        public MoneyColor Money { get; set; }
    }
    public class MoneyColor
    {
        public string Color { get; set; }
        public double Amount { get; set; }
        public string AmountFormat { get; set; }
    }

    // ko dùng
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
        public double TotalMoney { get; set; }
        public string Code { get; set; }
        public string Type { get; set; }
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
        public string password { get; set; }
        public string new_password { get; set; }
        public string password_confirmation { get; set; }
    }

    public class UserAvatarName
    {
        public string Avatar { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public int Role { get; set; }
    }

    public class UserDebtReturn
    {
        public int UserDeptId { get; set; }
        public string ReceiptName { get; set; }
        public string Date { get; set; }
        public string OwnerName { get; set; }
        public double DebtLeft { get; set; }
        public string DebtLeftFormat { get; set; }
        public int status { get; set; }
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
        public string Phone { get; set; }
    }
    public class IdAvatarNamePhone
    {
        public int UserId { get; set; }
        public string Avatar { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public int FriendStatus { get; set; }
        public int Role { get; set; }
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

    public class EventUserIDRole
    {
        public int EventId { get; set; }
        public int UserId { get; set; }
        public int Role { get; set; }
    }

    public class ReceiptSentParam
    {
        public int ReceiptId { get; set; }
        public string Date { get; set; }
        public string ReceiptName { get; set; }
        public double ReceiptAmount { get; set; }
        public string ReceiptAmountFormat { get; set; }
        public int ReceiptStatus { get; set; }
        public List<string> ImageLinks { get; set; }
        public UserAvatarName User { get; set; }
    }

    public class UserAvatarNameMoney
    {
        public string Avatar { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public int Role { get; set; }
        public double TotalAmount { get; set; }
        public string TotalAmountFormat { get; set; }
    }

    public class ReceiptUserDeptName
    {
        public string ReceiptName { get; set; }
        public string Date { get; set; }

        public UserAvatarNameMoney User { get; set; }
        public List<UserAvatarNameMoney> UserDepts { get; set; }
        public List<string> ImgLink { get; set; }
        public int ReceiptStatus { get; set; }
    }
    public class DebtPaymentPending
    {
        public int PaidDebtId { get; set; }
        public double TotalMoney { get; set; }
        public string TotalMoneyFormat { get; set; }
        public string Date { get; set; }
        public string Code { get; set; }
        public UserAvatarName User { get; set; }
        public string ImageLink { get; set; }
        public string Type { get; set; }
        public int Status { get; set; }
    }

    public class ListIdStatus
    {
        public List<int> ListId { get; set; }
        public int Status { get; set; }
    }

    public class JoinRequestHistory
    {
        public string Date { get; set; }
        public string Avatar { get; set; }
        public string UserName { get; set; }
        public string Phone { get; set; }
        public int Status { get; set; }
    }
    public class UserJoinRequestWaiting
    {
        public int RequestId { get; set; }
        public string Avatar { get; set; }
        public string UserName { get; set; }
        public string Phone { get; set; }

    }

    public class IdAvatarNamePhoneMoney
    {
        public int ReceiptId { get; set; }
        public string Avatar { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public int Role { get; set; }
        public string Money { get; set; }
    }
    public class TotalMoneyUser
    {
        public string Amount { get; set; }
        public List<IdAvatarNamePhoneMoney> List { get; set; }
    }

    public class ActivityScreen
    {
        public string? Link { get; set; }
        public string Content { get; set; }
        public DateTime Date { get; set; }
    }

    public class PaidDebtDetailScreen
    {
        public string Code { get; set; }
        public DateTime Date { get; set; }
        public List<UserAvatarNameMoney> Users { get; set; }
        public string ImgLink { get; set; }
    }

    public class ReportReturn
    {
        public int ID { get; set; }
        public int ReportReceiptID { get; set; }
        public string ReportReceiptName { get; set; }
        public int ReportStatus { get; set; }
        public string ReportReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public Member Reporter { get; set; }
    }

    public class ListURL
    {
        public List<string> listUrl { get; set; }
    }

    public class searchFriendToAdd
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserAvatar { get; set; }
        public string UserPhone { get; set; }
        public int AllowAddFriendStatus { get; set; }
    }

    public class Search
    {
        public string SearchText { get; set; }
    }
}
