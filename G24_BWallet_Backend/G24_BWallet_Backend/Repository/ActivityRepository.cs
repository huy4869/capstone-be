using Amazon.S3.Model;
using G24_BWallet_Backend.DBContexts;
using G24_BWallet_Backend.Models;
using G24_BWallet_Backend.Models.ObjectType;
using G24_BWallet_Backend.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace G24_BWallet_Backend.Repository
{
    public class ActivityRepository : IActivityRepository
    {
        private readonly MyDBContext context;
        private readonly Format format;

        public ActivityRepository(MyDBContext myDB)
        {
            this.context = myDB;
            format = new Format();
        }

        public async Task AddActivity(int userId, string content, string iconType)
        {
            try
            {
                Activity activity = new Activity();
                activity.UserID = userId;
                activity.Content = content;
                activity.CreatedAt = System.DateTime.Now;
                activity.UpdatedAt = System.DateTime.Now;
                if (iconType != null)
                {
                    ActivityIcon activityIcon = await context.ActivityIcons
                            .FirstOrDefaultAsync(a => a.Type.Equals(iconType.Trim()));
                    if (activityIcon != null)
                        activity.ActivityIconId = activityIcon.ID;
                }
                await context.Activities.AddAsync(activity);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<List<ActivityScreen>> GetActivity(int userId)
        {
            List<ActivityScreen> list = new List<ActivityScreen>();
            var activities = await context.Activities.Include(a => a.ActivityIcon)
                .OrderByDescending(a => a.ID)
                .Where(a => a.UserID == userId).ToListAsync();
            foreach (var activity in activities)
            {
                ActivityScreen activityScreen = new ActivityScreen();
                if (activity.ActivityIcon != null)
                    activityScreen.Link = activity.ActivityIcon.Link;
                activityScreen.Content = activity.Content.ToString();
                activityScreen.Date = format.DateFormat(activity.CreatedAt);
                list.Add(activityScreen);
            }
            return list;
        }

        public async Task InspectorReceiptApproveActivity(int status, int userId,
            string receiptName, string eventName)
        {
            string statuss = (status == 2) ? "phê duyệt" : "từ chối";
            string content = "Bạn đã " + statuss + " chứng từ <b>" + receiptName + "</b> nhóm <b>" + eventName + "</b>.";
            await AddActivity(userId, content, "receipt");
        }

        public async Task CreatorReceiptApproveActivity(int status, int userId,
            string receiptName, string eventName)
        {
            string statuss = (status == 2) ? "được phê duyệt" : "bị từ chối";
            string content = string.Format("Chứng từ <b>{0}</b> trong nhóm <b>{1}</b> của bạn đã {2}.", receiptName,
                eventName, statuss);
            await AddActivity(userId, content, "receipt");
        }

        public async Task AddReceiptActivity(int userID, string receiptName, int eventID)
        {
            Event e = await context.Events.FirstOrDefaultAsync(e => e.ID == eventID);
            string content = string.Format("Chứng từ <b>{0}</b> trong nhóm <b>{1}</b>" +
                " bạn mới thêm đang chờ duyệt.", receiptName, e.EventName);
            await AddActivity(userID, content, "receipt");
        }

        public async Task CreatorPaidDebtActivity(int userId, double totalMoney, int eventId)
        {
            Event e = await context.Events.FirstOrDefaultAsync(e => e.ID == eventId);
            string content = string.Format("Yêu cầu trả <b>{0}</b> của bạn trong sự kiện <b>{1}</b> đang" +
                " chờ duyệt."
                , format.MoneyFormat(totalMoney), e.EventName);
            await AddActivity(userId, content, "paidDebt");
        }

        public async Task CreatorPaidDebtApprovedActivity(int paidid, int userId, int status)
        {
            PaidDept paidDept = await context.PaidDepts
                .Include(p => p.Event)
                .FirstOrDefaultAsync(p => p.Id == paidid);
            string statuss = (status == 2) ? "được phê duyệt" : "bị từ chối";
            string content = string.Format("Yêu cầu trả <b>{0}</b> của bạn trong sự kiện <b>{1}</b>" +
                " đã {2}."
                , format.MoneyFormat(paidDept.TotalMoney), paidDept.Event.EventName, status);
            await AddActivity(userId, content, "paidDebt");
        }

        public async Task InspectorPaidDebtApprovedActivity(int paidid, int userId, int status)
        {
            PaidDept paidDept = await context.PaidDepts
                 .Include(p => p.Event)
                 .Include(p => p.User)
                 .FirstOrDefaultAsync(p => p.Id == paidid);
            User user = await context.Users.Include(u => u.Account)
                .FirstOrDefaultAsync(u => u.ID == paidDept.UserId);
            string statuss = (status == 2) ? "phê duyệt" : "từ chối";
            string content = string.Format("Bạn đã {0} yêu cầu trả <b>{1}</b> của <b>{2}({3})</b> " +
                "trong nhóm <b>{4}</b>"
                , statuss, format.MoneyFormat(paidDept.TotalMoney), paidDept.User.UserName,
                user.Account.PhoneNumber, paidDept.Event.EventName);
            await AddActivity(userId, content, "paidDebt");
        }

        // activity của event
        public async Task EventActivity(int status, int userId, string eventName)
        {
            string content = "";
            // sẽ có 3 activity: 1 là tạo event, 2 là đóng event, 3 là thoát
            if (status == 1)
                content = string.Format("Bạn đã tạo sự kiện <b>{0}</b>.", eventName);
            else if (status == 2)
                content = string.Format("Bạn đã đóng sự kiện <b>{0}</b>.", eventName);
            else if (status == 3)
                content = string.Format("Bạn đã rời sự kiện <b>{0}</b>.", eventName);
            await AddActivity(userId, content, "event");
        }

        // activity của request
        public async Task RequestActivity(int status, int acceptStatus, int userId, string eventName
            , int creatorId)
        {
            User user = null;
            if (creatorId != -1)
                user = await context.Users.Include(u => u.Account)
                    .FirstOrDefaultAsync(a => a.ID == creatorId);
            string content = "";
            // sẽ có 3 status: 1 là tạo request, 2 là thằng owner phê duyệt or từ chối,
            // 3 là thằng tạo nhận đc phản hồi phê duyệt or từ chối
            if (status == 1)
                content = string.Format("Yêu cầu tham gia sự kiện <b>{0}</b>" +
                    " đang chờ duyệt.", eventName);
            else if (status == 2 && acceptStatus == 1)
                content = string.Format("Bạn đã đồng ý yêu cầu tham gia sự kiện <b>{0}</b>" +
                    " của <b>{1}({2})</b>", eventName, user.UserName, user.Account.PhoneNumber);
            else if (status == 2 && acceptStatus == 0)
                content = string.Format("Bạn đã từ chối yêu cầu tham gia sự kiện <b>{0}</b>" +
                    " của <b>{1}({2})</b>", eventName, user.UserName, user.Account.PhoneNumber);
            else if (status == 3 && acceptStatus == 1)
                content = string.Format("Yêu cầu tham gia sự kiện <b>{0}</b>" +
                    "đã được chấp thuận.", eventName);
            else if (status == 3 && acceptStatus == 0)
                content = string.Format("Yêu cầu tham gia sự kiện <b>{0}</b>" +
                    "đã bị từ chối.", eventName);
            await AddActivity(userId, content, "request");
        }

        // activity của report
        public async Task ReportActivity(int status, int acceptStatus, int userId, string receiptName,
            string eventName)
        {
            string content = "";
            // sẽ có 3 status: 1 là tạo report, 2 là thằng owner phê duyệt or từ chối,
            // 3 là thằng tạo nhận đc phản hồi phê duyệt or từ chối
            if (status == 1)
                content = string.Format("Báo cáo hoá đơn <b>{0}</b> trong sự kiện <b>{1}</b>" +
                    " đang chờ duyệt.", receiptName, eventName);
            else if (status == 2 && acceptStatus == 1)
                content = string.Format("Bạn đã đồng ý báo cáo <b>{0}</b>" +
                    " trong sự kiện <b>{1}</b>.", receiptName, eventName);
            else if (status == 2 && acceptStatus == 0)
                content = string.Format("Bạn đã từ chối báo cáo <b>{0}</b>" +
                    " trong sự kiện <b>{1}</b>.", receiptName, eventName);
            else if (status == 3 && acceptStatus == 1)
                content = string.Format("Báo cáo hoá đơn <b>{0}</b> trong sự kiện <b>{1}</b>" +
                    " đã được duyệt.", receiptName, eventName);
            else if (status == 3 && acceptStatus == 0)
                content = string.Format("Báo cáo hoá đơn <b>{0}</b> trong sự kiện <b>{1}</b>" +
                    " đã bị từ chối.", receiptName, eventName);
            await AddActivity(userId, content, "report");
        }

        // activity của friend
        public async Task FriendActivity(int status, int acceptStatus, int userId, int friendId)
        {
            string content = "";
            User user = await context.Users.Include(u => u.Account)
                .FirstOrDefaultAsync(u => u.ID == userId);
            User friend = await context.Users.Include(u => u.Account)
               .FirstOrDefaultAsync(u => u.ID == friendId);
            // sẽ có 4 status: 1 là gửi lời mời kết bạn, 2 là thằng bạn chấp thuận or từ chối,
            // 3 là thằng gửi nhận đc phản hồi chấp thuận or từ chối, 4 là xoá bạn
            if (status == 1)
                content = string.Format("Đã gửi lời mời kết bạn đến <b>{0}({1})</b>.", friend.UserName,
                    friend.Account.PhoneNumber);
            else if (status == 2 && acceptStatus == 1)
                content = string.Format("Bạn đã đồng ý kết bạn với <b>{0}({1})</b>.",
                    friend.UserName, friend.Account.PhoneNumber);
            else if (status == 2 && acceptStatus == 0)
                content = string.Format("Bạn đã từ chối kết bạn với <b>{0}({1})</b>.", friend.UserName
                    , friend.Account.PhoneNumber);
            else if (status == 3 && acceptStatus == 1)
                content = string.Format("<b>{0}({1})</b> đã chấp nhận lời mời kết bạn.", friend.UserName,
                    friend.Account.PhoneNumber);
            else if (status == 3 && acceptStatus == 0)
                content = string.Format("<b>{0}({1})</b> đã từ chối lời mời kết bạn.", friend.UserName,
                    friend.Account.PhoneNumber);
            else if (status == 4)
                content = string.Format("Đã huỷ kết bạn với <b>{0}({1})</b>.", friend.UserName,
                    friend.Account.PhoneNumber);
            await AddActivity(userId, content, "friend");
        }

        // activity của invite
        public async Task InviteActivity(int status, int acceptStatus, int currentId, int otherId,
            int eventId)
        {
            string content = "";
            Event eventt = await context.Events.FirstOrDefaultAsync(e => e.ID == eventId);
            User user = await context.Users.Include(u => u.Account)
                .FirstOrDefaultAsync(u => u.ID == currentId);
            User friend = await context.Users.Include(u => u.Account)
                  .FirstOrDefaultAsync(u => u.ID == otherId);
            // sẽ có 4 status: 1 là thằng mời, 2 là thằng được mời nhận thông báo,
            // 3 là thằng đc mời chấp nhận hay từ chối,
            // 4 là thằng mời nhận phản hồi chấp nhận or từ chối
            if (status == 1)
                content = string.Format("Bạn đã mời <b>{0}({1})</b> tham gia sự kiện <b>{2}</b>.",
                    friend.UserName, friend.Account.PhoneNumber, eventt.EventName);
            else if (status == 2)
                content = string.Format("<b>{0}({1})</b> đã mời bạn tham gia sự kiện <b>{2}</b>."
                    , friend.UserName, friend.Account.PhoneNumber, eventt.EventName);
            else if (status == 3 && acceptStatus == 0)
                content = string.Format("Bạn đã từ chối tham gia sự kiện <b>{0}</b>."
                    , eventt.EventName);
            else if (status == 3 && acceptStatus == 1)
                content = string.Format("Bạn đã tham gia sự kiện <b>{0}</b>."
                    , eventt.EventName);
            else if (status == 4 && acceptStatus == 0)
                content = string.Format("<b>{0}({1})</b> đã từ chối tham gia sự kiện <b>{2}</b>."
                    , friend.UserName, friend.Account.PhoneNumber, eventt.EventName);
            else if (status == 4 && acceptStatus == 1)
                content = string.Format("<b>{0}({1})</b> đã tham gia sự kiện <b>{2}</b>."
                    , friend.UserName, friend.Account.PhoneNumber, eventt.EventName);
            await AddActivity(currentId, content, "invite");
        }
    }
}
