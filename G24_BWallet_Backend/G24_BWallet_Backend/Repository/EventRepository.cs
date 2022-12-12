using G24_BWallet_Backend.DBContexts;
using G24_BWallet_Backend.Models;
using G24_BWallet_Backend.Models.ObjectType;
using G24_BWallet_Backend.Repository.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Twilio.TwiML.Fax;

namespace G24_BWallet_Backend.Repository
{
    public class EventRepository : IEventRepository
    {
        private readonly MyDBContext context;
        private readonly Format format;
        private readonly IMemberRepository memberRepository;

        public EventRepository(MyDBContext myDB, IMemberRepository memberRepository)
        {
            this.context = myDB;
            this.format = new Format();
            this.memberRepository = memberRepository;
        }

        public async Task<int> AddEventAsync(Event e)
        {
            DateTime VNDateTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
            e.EventStatus = 1;
            e.CreatedAt = VNDateTime;
            e.UpdatedAt = VNDateTime;
            await context.Events.AddAsync(e);
            await context.SaveChangesAsync();
            return e.ID;
        }

        public async Task AddEventMember(int eventID, List<int> memebers)
        {
            int count = 0;
            foreach (var item in memebers)
            {
                EventUser eu = new EventUser();
                eu.EventID = eventID;
                eu.UserID = item;
                eu.UserRole = (count == memebers.Count - 1) ? 1 : 0;
                count++;
                await context.EventUsers.AddAsync(eu);
            }
            await context.SaveChangesAsync();
        }

        public async Task<bool> CheckUserJoinEvent(EventUserID eu)
        {
            EventUser eventUser = await context.EventUsers.FirstOrDefaultAsync(e =>
            e.EventID == eu.EventId && e.UserID == eu.UserId);
            if (eventUser == null)
                return false;
            return true;
        }

        public async Task<string> CreateEventUrl(int eventID)
        {
            // mã hoá event id
            string eventIdEncrypt = await format.EncryptAsync(eventID.ToString());
            string eventUrl = "/event/join/eventId=" + eventIdEncrypt;
            Event e = await context.Events.FirstOrDefaultAsync(e => e.ID == eventID);
            e.EventLink = eventUrl;
            await context.SaveChangesAsync();
            return eventUrl;
        }

        public async Task<List<EventHome>> GetAllEventsAsync(int userID, string name)
        {
            List<EventHome> events = new List<EventHome>();
            // lấy tất cả các event mà mình tham gia
            var listEvent = await context.EventUsers
                .Where(eu => eu.UserID == userID)
                .Select(eu => eu.Event)
                .OrderByDescending(eu => eu.ID)
                .ToListAsync();
            //nếu name có thì lấy theo name
            if (name != null)
            {
                listEvent = await GetEventUserByEventName(userID, name);
            }
            foreach (var eventt in listEvent)
            {
                EventHome eh = new EventHome();
                eh.EventId = eventt.ID;
                eh.EventLogo = eventt.EventLogo;
                eh.EventName = eventt.EventName;
                eh.EventStatus = eventt.EventStatus;
                eh.Debt = await GetDebtMoney(eventt.ID, userID);
                eh.Receive = await GetReceiveMoney(eventt.ID, userID);
                eh.TotalMoney = await GetTotalMoney(eh.Debt, eh.Receive);
                eh.ReceiptCount = await ReceiptCount(eventt.ID);
                events.Add(eh);
            }

            return events;
        }

        private async Task<int> ReceiptCount(int eventiD)
        {
            List<Receipt> receipts = await context.Receipts
                 .Where(r => r.EventID == eventiD && (r.ReceiptStatus == 2
                 || r.ReceiptStatus == 0 || r.ReceiptStatus == 4)).ToListAsync();
            return receipts.Count;
        }

        private async Task<List<Event>> GetEventUserByEventName(int userID, string name)
        {
            List<Event> userJoin = new List<Event>();
            List<Event> events = await context.Events
                .Where(e => e.EventName.Contains(name)).ToListAsync();
            foreach (var eventt in events)
            {
                EventUser eventUser = await context.EventUsers
                    .Include(eu => eu.Event)
                .FirstOrDefaultAsync(eu => eu.UserID == userID && eu.EventID == eventt.ID);
                if (eventUser != null)
                    userJoin.Add(eventUser.Event);

            }
            userJoin.Reverse();
            return userJoin;
        }

        private async Task<MoneyColor> GetTotalMoney(NumberMoney debt, NumberMoney receive)
        {
            MoneyColor money = new MoneyColor();
            if (debt.Money.Amount > receive.Money.Amount)
            {
                money.Color = "Red";
                money.Amount = debt.Money.Amount - receive.Money.Amount;
                money.AmountFormat = format.MoneyFormat(money.Amount);
            }
            else if (debt.Money.Amount < receive.Money.Amount)
            {
                money.Color = "Green";
                money.Amount = receive.Money.Amount - debt.Money.Amount;
                money.AmountFormat = format.MoneyFormat(money.Amount);
            }
            else if (debt.Money.Amount == receive.Money.Amount)
            {
                money.Color = "Gray";
                money.Amount = receive.Money.Amount - debt.Money.Amount;
                money.AmountFormat = format.MoneyFormat(money.Amount);
            }
            return await Task.FromResult(money);
        }

        // tiền người ta nợ mình trong event này
        private async Task<NumberMoney> GetReceiveMoney(int eventId, int userID)
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
                    && u.DeptStatus == 2 && u.DebtLeft > 0).ToListAsync();
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

        // tiền mình nợ trong event này
        private async Task<NumberMoney> GetDebtMoney(int eventId, int userID)
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
                    && u.DeptStatus == 2 && u.DebtLeft > 0);
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

        //private double GetTotalMoneyEachEvent(List<UserHome> userHomes)
        //{
        //    double totalDeptor = 0;
        //    double totalOwner = 0;
        //    foreach (var item in userHomes)
        //    {
        //        if (item.MoneyColor.Equals("Red"))
        //        {
        //            totalDeptor += item.Money;
        //        }
        //        else
        //        {
        //            totalOwner += item.Money;
        //        }
        //    }
        //    return (totalDeptor > totalOwner) ? (totalDeptor - totalOwner)
        //        : (totalOwner - totalDeptor);
        //}

        private async Task<List<UserHome>> GetUserHome(Event eventt, int userID)
        {
            List<UserHome> userHomes = new List<UserHome>();
            // lấy tất cả receipt chưa thanh toán trong event này
            var listReceipt = await context.Receipts.Where(r => r.EventID == eventt.ID
                && r.ReceiptStatus != 0).ToListAsync();
            //danh sách người mình nợ trong event
            List<UserDept> deptors = await GetDeptors(listReceipt, userID);
            //danh sách người nợ mình trong event
            List<UserDept> owener = await GetOwerners(listReceipt, userID);
            double totalDeptor = 0;
            double totalOwner = 0;
            if (deptors.Count != 0)
                foreach (var item in deptors)
                {
                    totalDeptor += item.DebtLeft;
                }
            if (owener.Count != 0)
                foreach (var item in owener)
                {
                    totalOwner += item.DebtLeft;
                }
            if (deptors.Count != 0 && owener.Count != 0)
            {
                UserHome uh1 = new UserHome();
                uh1.Avatar = deptors[0].User.Avatar;
                uh1.UserName = await GetUserNameByReceipt(deptors[0].Receipt);
                uh1.Money = deptors[0].DebtLeft;
                uh1.MoneyColor = "Red";
                userHomes.Add(uh1);
                totalDeptor -= uh1.Money;

                UserHome uh2 = new UserHome();
                uh2.Avatar = owener[0].User.Avatar;
                uh2.UserName = owener[0].User.UserName;
                uh2.Money = owener[0].DebtLeft;
                uh2.MoneyColor = "Green";
                userHomes.Add(uh2);
                totalOwner -= uh2.Money;
            }
            if (deptors.Count != 0 && owener.Count == 0)
            {
                for (int i = 0; i < deptors.Count; i++)
                {
                    UserHome uh1 = new UserHome();
                    uh1.Avatar = deptors[i].User.Avatar;
                    uh1.UserName = await GetUserNameByReceipt(deptors[0].Receipt);
                    uh1.Money = deptors[i].DebtLeft;
                    uh1.MoneyColor = "Red";
                    userHomes.Add(uh1);
                    totalDeptor -= uh1.Money;
                    if (i == 1) break;
                }
            }
            if (deptors.Count == 0 && owener.Count != 0)
            {
                for (int i = 0; i < owener.Count; i++)
                {
                    UserHome uh2 = new UserHome();
                    uh2.Avatar = owener[i].User.Avatar;
                    uh2.UserName = owener[i].User.UserName;
                    uh2.Money = owener[i].DebtLeft;
                    uh2.MoneyColor = "Green";
                    userHomes.Add(uh2);
                    totalOwner -= uh2.Money;
                    if (i == 1) break;
                }
            }
            if (userHomes.Count == 2 && (deptors.Count + owener.Count > 2))
            {
                UserHome uh3 = new UserHome();
                uh3.UserName = "Và " + (deptors.Count + owener.Count - 2) + " người khác";
                if (totalDeptor > totalOwner)
                {
                    uh3.Money = totalDeptor - totalOwner;
                    uh3.MoneyColor = "Red";
                }
                else
                {
                    uh3.Money = totalOwner - totalDeptor;
                    uh3.MoneyColor = "Green";
                }
                userHomes.Add(uh3);
            }
            return await Task.FromResult(userHomes);
        }

        private async Task<string> GetUserNameByReceipt(Receipt receipt)
        {
            Receipt receiptt = await context.Receipts.Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == receipt.Id);
            return receipt.User.UserName;
        }

        private async Task<List<UserDept>> GetOwerners(List<Receipt> listReceipt, int userID)
        {
            List<Receipt> receipts = listReceipt.Where(x => x.UserID == userID).ToList();
            List<UserDept> list = new List<UserDept>();
            foreach (var item in receipts)
            {
                UserDept ud = await context.UserDepts.Include(u => u.User).FirstOrDefaultAsync(u =>
                u.ReceiptId == item.Id);
                if (ud != null)
                    list.Add(ud);
            }
            list.Reverse();
            return list;
        }

        private async Task<List<UserDept>> GetDeptors(List<Receipt> listReceipt, int userID)
        {
            List<UserDept> list = new List<UserDept>();
            foreach (var item in listReceipt)
            {
                UserDept ud = await context.UserDepts.Include(u => u.User)
                    .Include(u => u.Receipt)
                    .FirstOrDefaultAsync(u =>
                u.ReceiptId == item.Id && u.UserId == userID);
                if (ud != null)
                    list.Add(ud);
            }
            list.Reverse();
            return list;
        }

        public async Task<string> GetEventUrl(int eventId)
        {
            var eventt = await context.Events.FirstOrDefaultAsync(e => e.ID == eventId);
            return eventt.EventLink;
        }

        public async Task<Event> GetEventById(int eventId)
        {
            return await context.Events.FirstOrDefaultAsync(e => e.ID == eventId);
        }

        // lấy danh sách các thành viên trong event
        public async Task<List<UserAvatarName>> GetListUserInEvent(int eventId)
        {
            List<UserAvatarName> result = new List<UserAvatarName>();
            List<EventUser> eu = await context.EventUsers.Include(e => e.User)
                .Where(e => e.EventID == eventId).ToListAsync();
            // săp xếp cho thằng owner lên đầu danh sách
            eu = await memberRepository.SortOwnerFirst(eu);
            eu.ForEach(async item => result.Add(
                new UserAvatarName
                {
                    Avatar = item.User.Avatar,
                    Name = item.User.UserName,
                    Phone = await memberRepository.GetPhoneByUserId(item.User.ID),
                    Role = await memberRepository.GetRole(eventId, item.User.ID)
                }));
            return result;
        }

        public async Task<string> SendJoinRequest(EventUserID eventUserID)
        {
            EventUser eu = await context.EventUsers
                .FirstOrDefaultAsync(e => e.UserID == eventUserID.UserId
                && e.EventID == eventUserID.EventId);
            if (eu != null) return "Bạn đã ở trong event này rồi";

            DateTime VNDateTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
            Request r = new Request();
            r.UserID = eventUserID.UserId;
            r.EventID = eventUserID.EventId;
            r.Status = 3;
            r.CreatedAt = VNDateTime;
            r.UpdatedAt = VNDateTime;
            var request = await context.Requests
                .FirstOrDefaultAsync(request => request.UserID == r.UserID
                && request.EventID == r.EventID);
            if (request != null && request.Status == 3)
                return "Bạn đã gửi yêu cầu gia nhập nhóm này rồi(đang chờ accept)";
            if (request != null && request.Status == 5)
            {
                request.Status = 3;
                request.UpdatedAt = VNDateTime;
                await context.SaveChangesAsync();
                return "Gửi lại yêu cầu gia nhập nhóm thành công, đang chờ duyệt";
            }
            await context.Requests.AddAsync(r);
            await context.SaveChangesAsync();
            return "Gửi yêu cầu gia nhập nhóm thành công, đang chờ duyệt";
        }

        // các yêu cầu tham gia đang chờ duyệt
        public async Task<List<UserJoinRequestWaiting>> GetJoinRequest(int eventId)
        {
            List<UserJoinRequestWaiting> request = new List<UserJoinRequestWaiting>();
            List<Request> requests = await context.Requests
                .Include(request => request.User)
                .Where(request => request.EventID == eventId
                && request.Status == 3).ToListAsync();
            foreach (Request item in requests)
            {
                UserJoinRequestWaiting u = new UserJoinRequestWaiting();
                u.RequestId = item.Id;
                u.Avatar = item.User.Avatar;
                u.UserName = item.User.UserName;
                Account acc = await context.Accounts.FirstOrDefaultAsync(acc =>
                acc.ID == item.User.AccountID);
                u.Phone = acc.PhoneNumber;
                request.Add(u);
            }
            return request;
        }

        public async Task UpdateEventInformation(EventIdNameDes e)
        {
            Event eventt = await GetEventById(e.EventId);
            if (e.EventName.Trim().Length != 0)
                eventt.EventName = e.EventName.Trim();
            if (e.EventDescript.Trim().Length != 0)
                eventt.EventDescript = e.EventDescript.Trim();
            await context.SaveChangesAsync();
        }

        public async Task ApproveEventJoinRequest(ListIdStatus list)
        {
            foreach (int item in list.ListId)
            {
                Request request = await context.Requests.FirstOrDefaultAsync(p => p.Id == item);
                request.Status = list.Status;
                await context.SaveChangesAsync();
                if (list.Status == 4)
                {
                    // accept
                    await AddUserToEvent(request);
                }
            }
        }

        private async Task AddUserToEvent(Request request)
        {
            EventUser e = await context.EventUsers
                .FirstOrDefaultAsync(e => e.EventID == request.EventID && e.UserID == request.UserID);
            if (e == null)
            {
                EventUser eventUser = new EventUser
                {
                    UserID = request.UserID,
                    EventID = request.EventID,
                    UserRole = 0
                };
                await context.EventUsers.AddAsync(eventUser);
                await context.SaveChangesAsync();
            }
        }

        // lịch sử yêu cầu tham gia
        public async Task<List<JoinRequestHistory>> JoinRequestHistory(int eventId)
        {
            List<JoinRequestHistory> result = new List<JoinRequestHistory>();
            // lấy các request đã chấp nhận hoặc từ chối
            List<Request> requests = await context.Requests
                .Include(request => request.User)
                .OrderByDescending(request => request.Id)
                .Where(request => request.EventID == eventId && request.Status != 0
                && request.Status != 3).ToListAsync();
            foreach (Request item in requests)
            {
                JoinRequestHistory u = new JoinRequestHistory();
                u.Date = item.CreatedAt.ToString();
                u.Avatar = item.User.Avatar;
                u.UserName = item.User.UserName;
                Account acc = await context.Accounts.FirstOrDefaultAsync(acc =>
                acc.ID == item.User.AccountID);
                u.Phone = acc.PhoneNumber;
                u.Status = item.Status;
                result.Add(u);
            }
            return result;
        }

        public async Task<string> CloseEvent(int userId, int eventId)
        {
            EventUser eventUser = await context.EventUsers
                .FirstOrDefaultAsync(e => e.EventID == eventId && e.UserID == userId);
            // inspector muốn out nhóm thì phải check ko còn 1 hoá đơn nào chờ duyệt
            if (eventUser.UserRole == 2)
            {
                Receipt receipt = await context.Receipts
                    .FirstOrDefaultAsync(r => r.EventID == eventId && r.ReceiptStatus == 1);
                if (receipt != null) return "Còn chứng từ chưa duyệt!";
            }
            // cashier muốn out nhóm thì phải check ko còn 1 paiddebt  nào chờ duyệt
            if (eventUser.UserRole == 3)
            {
                PaidDept paidDept = await context.PaidDepts
                   .FirstOrDefaultAsync(r => r.EventId == eventId && r.Status == 1);
                if (paidDept != null) return "Còn yêu cầu trả tiền chưa duyệt!";
            }
            // và tất cả mọi người muốn out nhóm thì phải ko còn nợ, và phải thu đủ tiền
            Receipt receiptActive = await context.Receipts
                    .FirstOrDefaultAsync(r => r.EventID == eventId && r.ReceiptStatus == 2
                    && r.UserID == userId);
            if (receiptActive != null) return "Bạn còn chứng từ chưa thu đủ tiền!";
            PaidDept paidDeptActive = await context.PaidDepts
                   .FirstOrDefaultAsync(r => r.EventId == eventId && r.Status == 2 && r.UserId == userId);
            if (paidDeptActive != null) return "Bạn còn khoản nợ chưa trả!";
            // đủ điều kiện thì có thể out hoặc đóng event 
            if (eventUser.UserRole == 1)
            { // owner sẽ close event
                Event e = await context.Events.FirstOrDefaultAsync(ev => ev.ID == eventId);
                e.EventStatus = 0;
                await context.SaveChangesAsync();
                return "Đóng sự kiện thành công";
            }
            // các member còn lại thì xoá khỏi bảng EventUser là xong
            context.EventUsers.Remove(eventUser);
            await context.SaveChangesAsync();
            return "Rời sự kiện thành công";
        }

        public async Task<bool> IsMaxMember(int eventId)
        {
            List<EventUser> eventUsers = await context.EventUsers
                .Where(e => e.EventID == eventId).ToListAsync();
            if (eventUsers.Count == 500) return true;
            return false;
        }

        public async Task<IDictionary> GetEventStatus(int eventId)
        {
            Event e = await context.Events
                 .FirstOrDefaultAsync(e => e.ID == eventId);
            IDictionary<string, int> dictionary = new Dictionary<string, int>
            {
                { "EventStatus", e.EventStatus }
            };
            return (IDictionary)dictionary;
        }

        public async Task<List<Report>> GetReportWaiting(int eventId)
        {
            List<Report> reports = await context.Reports
                .Where(r => r.EventId == eventId).ToListAsync();
            return reports;
        }
    }
}
