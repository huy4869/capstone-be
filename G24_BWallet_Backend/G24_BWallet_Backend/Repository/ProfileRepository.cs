using G24_BWallet_Backend.DBContexts;
using G24_BWallet_Backend.Models;
using G24_BWallet_Backend.Models.ObjectType;
using G24_BWallet_Backend.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace G24_BWallet_Backend.Repository
{
    public class ProfileRepository : IProfileRepository
    {
        private readonly MyDBContext context;
        private readonly ActivityRepository activity;
        private readonly Format format;

        public ProfileRepository(MyDBContext myDB)
        {
            this.context = myDB;
            this.activity = new ActivityRepository(myDB);
            format = new Format();
        }

        // chấp nhận hay từ chối lời mời tham gia vào nhóm
        public async Task<bool> ChangeStatusInvite(InviteRespondParam ip, int userId)
        {
            Invite invite = await context.Invites
                .Include(i => i.Event)
                .FirstOrDefaultAsync(i => i.ID == ip.InviteId);
            if (ip.Status == 2)// từ chối
            {
                invite.Status = 2;
                await context.SaveChangesAsync();
                await activity.InviteActivity(3, 0, userId, -1, invite.EventID);
                await activity.InviteActivity(4, 0, invite.UserID, userId, invite.EventID);
                return false;
            }
            // chấp nhận
            invite.Status = 1;
            // add user vao event
            EventUser eu = new EventUser();
            eu.UserID = userId;
            eu.EventID = invite.EventID;
            eu.UserRole = 2;
            await context.EventUsers.AddAsync(eu);
            await context.SaveChangesAsync();
            await activity.InviteActivity(3, 1, userId, -1, invite.EventID);
            await activity.InviteActivity(4, 1, invite.UserID, userId, invite.EventID);
            return true;
        }

        // lấy các invite mà nhóm mời mình vào,status = 0,1,2
        public async Task<List<Invite>> GetInvitePending(int userId)
        {
            return await context.Invites.Where(r =>
            (r.UserID == userId || r.FriendId == userId)
            && (r.Status == 0 || r.Status == 1 || r.Status == 2))
                .ToListAsync();
        }

        // lấy các request mình xin vào nhóm, status = 3,4,5
        public async Task<List<Request>> GetRequestPending(int userId)
        {
            return await context.Requests.Where(r =>
            r.UserID == userId && (r.Status == 3 || r.Status == 4 || r.Status == 5))
                .ToListAsync();
        }


        public async Task<User> GetUserById(int userId)
        {
            return await context.Users.Include(u => u.Account)
                .FirstOrDefaultAsync(u => u.ID == userId);
        }

        public async Task<List<InviteJoinParam>> ShowInviteJoinEvent(int userID)
        {
            List<InviteJoinParam> list = new List<InviteJoinParam>();
            var invites = await context.Invites.Include(r => r.Event)
                .Where(r => r.FriendId == userID && r.Status == 0).ToListAsync();
            foreach (Invite item in invites)
            {
                InviteJoinParam r = new InviteJoinParam();
                r.InviteId = item.ID;
                r.EventLogo = item.Event.EventLogo;
                r.EventName = item.Event.EventName;
                r.Date = format.DateFormat(item.CreateAt);
                var user = await GetUserById(item.UserID);
                r.UserName = user.UserName;
                list.Add(r);
            }
            return list;
        }

        public async Task<List<RequestJoinParam>> ShowRequestJoinEvent(int userID)
        {
            List<RequestJoinParam> list = new List<RequestJoinParam>();
            var requests = await context.Requests.Include(r => r.Event)
                .Where(r => r.UserID == userID).ToListAsync();
            foreach (Request item in requests)
            {
                RequestJoinParam r = new RequestJoinParam();
                r.EventId = item.EventID;
                r.EventLogo = item.Event.EventLogo;
                r.EventName = item.Event.EventName;
                r.Date = format.DateFormat(item.CreatedAt);
                r.Status = item.Status;
                list.Add(r);
            }
            return list;
        }
    }
}
