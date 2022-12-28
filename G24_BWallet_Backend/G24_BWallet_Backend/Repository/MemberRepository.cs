using Amazon.S3.Model;
using G24_BWallet_Backend.DBContexts;
using G24_BWallet_Backend.Models;
using G24_BWallet_Backend.Models.ObjectType;
using G24_BWallet_Backend.Repository.Interface;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace G24_BWallet_Backend.Repository
{
    public class MemberRepository : IMemberRepository
    {
        private readonly MyDBContext context;
        private readonly Format format;
        //private readonly IEventRepository eventRepository;

        public MemberRepository(MyDBContext myDB)
        {
            this.context = myDB;
            this.format = new Format();
            //this.eventRepository = eventRepository;
        }

        // xoá phân quyền
        public async Task DeletePromoteMemberRole(EventUserID e)
        {
            EventUser eu = await context.EventUsers
               .FirstOrDefaultAsync(ee => ee.EventID == e.EventId && ee.UserID == e.UserId);
            // nếu thằng hiện tại là owner thì không xoá gì hết
            if (eu.UserRole == 1)
                return;
            // không thì cho hết xuống thành role = 0
            eu.UserRole = 0;
            await context.SaveChangesAsync();
        }

        public async Task<Event> GetEvent(int eventId)
        {
            return await context.Events
                .FirstOrDefaultAsync(e => e.ID == eventId);
        }

        public async Task<IDictionary> GetMemberRole(int eventId, int userId)
        {
            IDictionary<string, int> pairs = new Dictionary<string, int>();
            EventUser eventUser = await context.EventUsers
                .FirstOrDefaultAsync(e => e.EventID == eventId && e.UserID == userId);
            if (eventUser != null)
                pairs.Add("UserRole", eventUser.UserRole);
            return (IDictionary)pairs;
        }

        public async Task<bool> IsCashier(int eventId, int userId)
        {
            EventUser eu = await context.EventUsers
                .FirstOrDefaultAsync(ee => ee.EventID == eventId && ee.UserID == userId);
            if (eu.UserRole == 3) return true;
            else if (eu.UserRole == 1) return true;
            return false;
        }

        public async Task<bool> IsInspector(int eventId, int userId)
        {
            EventUser eu = await context.EventUsers
                .FirstOrDefaultAsync(ee => ee.EventID == eventId && ee.UserID == userId);
            if (eu.UserRole == 2) return true;
            else if (eu.UserRole == 1) return true;
            return false;
        }

        public async Task<bool> IsNormalMember(int eventId, int userId)
        {
            EventUser eu = await context.EventUsers
                 .FirstOrDefaultAsync(ee => ee.EventID == eventId && ee.UserID == userId);
            if (eu.UserRole == 0) return true;
            return false;
        }

        public async Task<bool> IsOwner(int eventId, int userId)
        {
            EventUser eu = await context.EventUsers
                .FirstOrDefaultAsync(ee => ee.EventID == eventId && ee.UserID == userId);
            //EventUser inspector = await context.EventUsers
            //    .FirstOrDefaultAsync(ee => ee.EventID == eventId && ee.UserRole == 2);
            //EventUser cashier = await context.EventUsers
            //    .FirstOrDefaultAsync(ee => ee.EventID == eventId && ee.UserRole == 3);
            //if (eu.UserRole == 1) return true;
            //if (inspector != null && inspector.UserID == userId) return true;
            //if (cashier != null && cashier.UserID == userId) return true;
            return eu.UserRole == 1;
        }

        public async Task PromoteMemberRole(EventUserIDRole e)
        {
            EventUser eu = await context.EventUsers
                .FirstOrDefaultAsync(ee => ee.EventID == e.EventId && ee.UserID == e.UserId);
            if (eu.UserRole != 1)// owner luôn luôn là 1
                eu.UserRole = e.Role;
            await context.SaveChangesAsync();
        }

        public async Task<int> InActiveMember(EventUserID e)
        {
            EventUser eu = await context.EventUsers
                .FirstOrDefaultAsync(ee => ee.EventID == e.EventId && ee.UserID == e.UserId);
            // trước khi inactive phải xem thằng này còn hoá đơn hoặc nợ chưa giải quyết không
            // lấy các hoá đơn đang chờ hoặc đang trả trong event: status = 1,2,4
            //List<Receipt> receipts = await context.Receipts
            //    .Where(r => r.EventID == e.EventId && (r.ReceiptStatus == 1
            //    || r.ReceiptStatus == 2 || r.ReceiptStatus == 4)).ToListAsync();
            //foreach (Receipt receipt in receipts)
            //{
            //    // nếu receipt này của user này tạo thì return luôn
            //    if (receipt.UserID == e.UserId)
            //        return 10;
            //    // nếu không tạo receipt thì xem nó có trong danh sách nợ chưa trả của
            //    // receipt này không, nếu có thì return luôn
            //    List<UserDept> userDepts = await context.UserDepts
            //        .Where(u => u.ReceiptId == receipt.Id && u.UserId == e.UserId
            //        && (u.DeptStatus == 1 || u.DeptStatus == 2 || u.DeptStatus == 4)
            //        && u.DebtLeft > 0).ToListAsync();
            //    if (userDepts != null && userDepts.Count > 0)
            //    {
            //        // nợ vẫn chưa trả xong
            //        return 11;
            //    }
            //}

            // đoạn này check đơn giản bằng cách lấy tổng mình nợ trong event,
            // và tổng mình cần thu trong event, cái nào lớn hơn thì chưa inactive đc,
            // bằng nhau thì đc inactive
            double allDebt = (await GetDebtMoney(e.EventId, e.UserId)).Money.Amount;
            double allReceive = (await GetReceiveMoney(e.EventId, e.UserId)).Money.Amount;
            if (allReceive > allDebt)
            {
                return 10;
            }
            else if (allReceive < allDebt)
            {
                return 11;
            }
            eu.UserRole = 4;
            await context.SaveChangesAsync();
            return 0;
        }

        // lấy ra màn chi tiết khi click vào danh sách thành viên
        public async Task<MemberDetailParam> ShowMemeberDetail(int eventId, int userId)
        {
            MemberDetailParam param = new MemberDetailParam();
            param.EventName = (await GetEvent(eventId)).EventName;

            // create inspector
            User inspector = await GetUserByRole(eventId, 2);
            if (inspector != null)
            {
                IdAvatarNameRole ins = new IdAvatarNameRole();
                ins.Avatar = inspector.Avatar;
                ins.UserId = inspector.ID;
                ins.Name = inspector.UserName;
                ins.Role = 2;
                ins.Phone = await GetPhoneByUserId(inspector.ID);
                param.Inspector = ins;
            }
            // create cashier
            User cashier = await GetUserByRole(eventId, 3);
            if (cashier != null)
            {
                IdAvatarNameRole cas = new IdAvatarNameRole();
                cas.Avatar = cashier.Avatar;
                cas.UserId = cashier.ID;
                cas.Name = cashier.UserName;
                cas.Role = 3;
                cas.Phone = await GetPhoneByUserId(cashier.ID);
                param.Cashier = cas;
            }

            param.Members = await GetListMember(eventId, userId);
            return param;
        }

        // lấy ra danh sách tất cả các thành viên trong event này, kể cả active hay không
        private async Task<List<IdAvatarNamePhone>> GetListMember(int eventId, int currentId)
        {
            List<IdAvatarNamePhone> list = new List<IdAvatarNamePhone>();
            List<EventUser> eventUsers = await context.EventUsers
                .Where(e => e.EventID == eventId).ToListAsync();
            // sắp xếp thằng owner lên đầu danh sách, cho mấy thằng inactive xuống cuối
            eventUsers = await SortList(eventUsers);
            foreach (EventUser user in eventUsers)
            {
                User u = await context.Users.Include(uu => uu.Account)
                    .FirstOrDefaultAsync(uu => uu.ID == user.UserID);
                IdAvatarNamePhone i = new IdAvatarNamePhone();
                i.UserId = u.ID;
                i.Avatar = u.Avatar;
                i.Name = u.UserName;
                i.Phone = u.Account.PhoneNumber;
                if (currentId != u.ID)
                    i.FriendStatus = await GetFriendStatus(currentId, u.ID);
                i.Role = user.UserRole;
                list.Add(i);
            }
            return list;
        }

        // current id là mình, user id là từng thằng trong list member
        private async Task<int> GetFriendStatus(int currentId, int userID)
        {
            User user = await context.Users.FirstOrDefaultAsync(u => u.ID == userID);
            Friend friend = await context.Friends
                .Where(f => (f.UserID == currentId && f.UserFriendID == userID)
                || (f.UserFriendID == currentId && f.UserID == userID)
                )
                .OrderBy(f => f.UserFriendID)
                .LastOrDefaultAsync();
            //có trong bảng rồi
            if (friend != null)
            {
                if (friend.status == 1)// là bạn rồi
                    return 3;
                if (friend.status == 0) // đã gửi kết bạn
                    return 2;
                if (friend.status == 2 && user.AllowAddFriendStatus == 1) // có thể kết bạn
                    return 1;
            }
            // không có trong bảng
            if (user.AllowAddFriendStatus == 1)// có thể kết bạn
                return 1;
            return 0; // không thế kết bạn
        }

        private async Task<User> GetUserByRole(int eventId, int role)
        {
            EventUser eventUser = await context.EventUsers.Include(e => e.User)
                .FirstOrDefaultAsync(e => e.EventID == eventId && e.UserRole == role);
            return eventUser?.User;
        }

        // danh sách các member thường để promote, role == 0
        public async Task<List<IdAvatarNamePhone>> ListPromote(int eventId, int userId, string name)
        {
            List<IdAvatarNamePhone> list = new List<IdAvatarNamePhone>();
            List<EventUser> eventUsers = await context.EventUsers
                .Include(e => e.User)
                .Where(e => e.EventID == eventId && e.UserRole == 0).ToListAsync();
            foreach (EventUser item in eventUsers)
            {
                IdAvatarNamePhone param = new IdAvatarNamePhone();
                param.UserId = item.UserID;
                param.Avatar = item.User.Avatar;
                param.Name = item.User.UserName;
                param.Phone = await GetPhoneByUserId(item.UserID);
                list.Add(param);
            }
            // kiểm tra nếu name == null thì trả về list luôn
            if (name.IsNullOrEmpty())
                return list;
            // không thì phải search
            name = format.SearchTextFormat(name.Trim());
            List<IdAvatarNamePhone> listSearch = new List<IdAvatarNamePhone>();
            list.ForEach(param =>
            {
                string userName = format.SearchTextFormat(param.Name);
                if (userName.Contains(name))
                    listSearch.Add(param);
            });
            return listSearch;
        }

        public async Task<string> GetPhoneByUserId(int useriD)
        {
            User user = await context.Users.Include(u => u.Account)
               .FirstOrDefaultAsync(u => u.ID == useriD);
            return user.Account.PhoneNumber;
        }

        public async Task<int> GetRole(int eventId, int userId)
        {
            EventUser eventUser = await context.EventUsers
                .FirstOrDefaultAsync(e => e.EventID == eventId && e.UserID == userId);
            return eventUser.UserRole;
        }

        // sắp xếp thằng owner lên đầu danh sách, cho mấy thằng inactive xuống cuối
        public async Task<List<EventUser>> SortList(List<EventUser> eventUsers)
        {
            // đầu tiên là cho mấy thằng inactive lên đầu, tí nữa mới đảo lại
            eventUsers = eventUsers.OrderByDescending(u => u.UserRole).ToList();
            EventUser eventUser = new EventUser();
            foreach (var item in eventUsers)
            {
                if (item.UserRole == 1)
                    eventUser = item;
            }
            eventUsers.Remove(eventUser);
            // lúc này thằng owner đang ở cuối, chỗ inactive đang trên cùng
            eventUsers.Add(eventUser);
            // đảo lại theo đúng trật tự: owner đầu tiên, inactive cuối cùng
            eventUsers.Reverse();
            return eventUsers;
        }

        // tiền mình nợ trong event này
        public async Task<NumberMoney> GetDebtMoney(int eventId, int userID)
        {
            NumberMoney number = new NumberMoney();
            MoneyColor moneyColor = new MoneyColor();
            double mon = 0;
            int total = 0;
            List<int> userIdList = new List<int>();
            // lấy hết các receipt đang trả trong event này
            List<Receipt> receiptList = await context.Receipts
                .Where(r => r.EventID == eventId && r.ReceiptStatus == 2).ToListAsync();
            foreach (Receipt receipt in receiptList)
            {
                UserDept userDept = await context.UserDepts
                    .FirstOrDefaultAsync(u => u.UserId == userID && u.ReceiptId == receipt.Id
                    && (u.DeptStatus == 2 || u.DeptStatus == 4) && u.DebtLeft > 0);
                if (userDept != null)
                {
                    mon += userDept.DebtLeft;
                    userIdList.Add(receipt.UserID);
                }
            }
            total = userIdList.Distinct().Count();
            moneyColor.Color = "Red";
            moneyColor.Amount = mon;
            moneyColor.AmountFormat = format.MoneyFormat(mon);
            number.Money = moneyColor;
            number.TotalPeople = total;
            return number;
        }

        // tiền người ta nợ mình trong event này
        public async Task<NumberMoney> GetReceiveMoney(int eventId, int userID)
        {
            NumberMoney number = new NumberMoney();
            MoneyColor moneyColor = new MoneyColor();
            double mon = 0;
            int total = 0;
            List<int> userIdList = new List<int>();
            // lấy hết các receipt mình tạo trong event này mà vẫn đang trả
            List<Receipt> receiptList = await context.Receipts
                .Where(r => r.EventID == eventId && r.ReceiptStatus == 2
                && r.UserID == userID).ToListAsync();
            foreach (Receipt receipt in receiptList)
            {
                List<UserDept> userDepts = await context.UserDepts
                    .Where(u => u.ReceiptId == receipt.Id
                    && (u.DeptStatus == 2 || u.DeptStatus == 4) && u.DebtLeft > 0).ToListAsync();
                foreach (var userDept in userDepts)
                {
                    if (userDept != null)
                    {
                        mon += userDept.DebtLeft;
                        userIdList.Add(userDept.UserId);
                    }
                }
            }
            total = userIdList.Distinct().Count();
            moneyColor.Color = "Green";
            moneyColor.Amount = mon;
            moneyColor.AmountFormat = format.MoneyFormat(mon);
            number.Money = moneyColor;
            number.TotalPeople = total;
            return number;
        }

    }
}
