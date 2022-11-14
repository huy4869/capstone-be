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

namespace G24_BWallet_Backend.Repository
{
    public class EventRepository : IEventRepository
    {
        private readonly MyDBContext context;

        public EventRepository(MyDBContext myDB)
        {
            this.context = myDB;
        }

        public async Task<int> AddEventAsync(Event e)
        {
            e.EventStatus = 1;
            e.CreatedAt = System.DateTime.Now;
            e.UpdatedAt = System.DateTime.Now;
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
                eu.UserRole = (count == 0) ? 1 : 2;
                count = 1;
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
            string eventUrl = "/event/join/eventId=" + eventID;
            Event e = await context.Events.FirstOrDefaultAsync(e => e.ID == eventID);
            e.EventLink = eventUrl;
            await context.SaveChangesAsync();
            return eventUrl;
        }

        public async Task<List<EventHome>> GetAllEventsAsync([FromBody] int userID)
        {
            List<EventHome> events = new List<EventHome>();
            // lấy tất cả các event mà mình tham gia
            var listEvent = await context.EventUsers
                .Where(eu => eu.UserID == userID)
                .Select(eu => eu.Event).ToListAsync();
            foreach (var eventt in listEvent)
            {
                EventHome eh = new EventHome();
                eh.EventId = eventt.ID;
                eh.EventLogo = eventt.EventLogo;
                eh.EventName = eventt.EventName;
                eh.ListUser = await GetUserHome(eventt, userID);
                eh.TotalMoney = GetTotalMoneyEachEvent(eh.ListUser);
                events.Add(eh);
            }
            return events;
        }

        private double GetTotalMoneyEachEvent(List<UserHome> userHomes)
        {
            double totalDeptor = 0;
            double totalOwner = 0;
            foreach (var item in userHomes)
            {
                if (item.MoneyColor.Equals("Red"))
                {
                    totalDeptor += item.Money;
                }
                else
                {
                    totalOwner += item.Money;
                }
            }
            return (totalDeptor > totalOwner) ? (totalDeptor - totalOwner)
                : (totalOwner - totalDeptor);
        }

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

        public async Task<Event> GetEventIntroduce(int eventId)
        {
            return await context.Events.FirstOrDefaultAsync(e => e.ID == eventId);
        }

        public async Task<List<UserAvatarName>> GetListUserInEvent(int eventId)
        {
            List<UserAvatarName> result = new List<UserAvatarName>();
            List<EventUser> eu = await context.EventUsers.Include(e => e.User)
                .Where(e => e.EventID == eventId).ToListAsync();
            eu.ForEach(item => result.Add(
                new UserAvatarName { Avatar = item.User.Avatar, Name = item.User.UserName }));
            return result;
        }

        public async Task<bool> SendJoinRequest(EventUserID eventUserID)
        {
            EventUser eu = await context.EventUsers
                .FirstOrDefaultAsync(e => e.UserID == eventUserID.UserId
                && e.EventID == eventUserID.EventId);
            if (eu != null) return false;
            Request r = new Request();
            r.UserID = eventUserID.UserId;
            r.EventID = eventUserID.EventId;
            r.Status = 3;
            r.CreatedAt = DateTime.Now;
            r.UpdatedAt = DateTime.Now;
            await context.Requests.AddAsync(r);
            await context.SaveChangesAsync();
            return true;
        }
    }
}
