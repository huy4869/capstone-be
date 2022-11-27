using Amazon.S3.Model;
using G24_BWallet_Backend.DBContexts;
using G24_BWallet_Backend.Models;
using G24_BWallet_Backend.Models.ObjectType;
using G24_BWallet_Backend.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace G24_BWallet_Backend.Repository
{
    public class MemberRepository : IMemberRepository
    {
        private readonly MyDBContext context;

        public MemberRepository(MyDBContext myDB)
        {
            this.context = myDB;
        }

        public async Task DeletePromoteMemberRole(EventUserID e)
        {
            EventUser eu = await context.EventUsers
               .FirstOrDefaultAsync(ee => ee.EventID == e.EventId && ee.UserID == e.UserId);
            // kiểm tra trong list event user có ông nào status là 1 không,
            // nếu không có thì ông hiện tại là 1, không thì role = 0
            EventUser owner = await context.EventUsers
               .FirstOrDefaultAsync(ee => ee.EventID == e.EventId && ee.UserRole == 1);
            if (owner != null)
            {
                eu.UserRole = 0;
            }
            else
            {
                eu.UserRole = 1;
            }
            await context.SaveChangesAsync();
        }

        public async Task<Event> GetEvent(int eventId)
        {
            return await context.Events
                .FirstOrDefaultAsync(e => e.ID == eventId);
        }

        public async Task<bool> IsOwner(int eventId, int userId)
        {
            EventUser eu = await context.EventUsers
                .FirstOrDefaultAsync(ee => ee.EventID == eventId && ee.UserID == userId);
            EventUser inspector = await context.EventUsers
                .FirstOrDefaultAsync(ee => ee.EventID == eventId && ee.UserRole == 2);
            EventUser cashier = await context.EventUsers
                .FirstOrDefaultAsync(ee => ee.EventID == eventId && ee.UserRole == 3);
            if (eu.UserRole == 1) return true;
            if (inspector != null && inspector.UserID == userId) return true;
            if (cashier != null && cashier.UserID == userId) return true;
            return false;
        }

        public async Task PromoteMemberRole(EventUserIDRole e)
        {
            EventUser eu = await context.EventUsers
                .FirstOrDefaultAsync(ee => ee.EventID == e.EventId && ee.UserID == e.UserId);
            eu.UserRole = e.Role;
            await context.SaveChangesAsync();
        }

        public async Task RemoveMember(EventUserID e)
        {
            EventUser eu = await context.EventUsers
                .FirstOrDefaultAsync(ee => ee.EventID == e.EventId && ee.UserID == e.UserId);
            context.EventUsers.Remove(eu);
            await context.SaveChangesAsync();
        }

        public async Task<MemberDetailParam> ShowMemeberDetail(int eventId)
        {
            MemberDetailParam param = new MemberDetailParam();
            param.EventName = (await GetEvent(eventId)).EventName;

            // create inspector
            User inspector = await GetUserByRole(eventId, 2);
            IdAvatarNameRole ins = new IdAvatarNameRole();
            ins.Avatar = inspector.Avatar;
            ins.UserId = inspector.ID;
            ins.Name = inspector.UserName;
            ins.Role = 2;
            param.Inspector = ins;

            // create cashier
            User cashier = await GetUserByRole(eventId, 3);
            if (cashier != null)
            {
                IdAvatarNameRole cas = new IdAvatarNameRole();
                cas.Avatar = cashier.Avatar;
                cas.UserId = cashier.ID;
                cas.Name = cashier.UserName;
                cas.Role = 3;
                param.Cashier = cas;
            }

            param.Members = await GetListMember(eventId);
            return param;
        }

        private async Task<List<IdAvatarNamePhone>> GetListMember(int eventId)
        {
            List<IdAvatarNamePhone> list = new List<IdAvatarNamePhone>();
            List<EventUser> eventUsers = await context.EventUsers
                .Where(e => e.EventID == eventId).ToListAsync();
            foreach (EventUser user in eventUsers)
            {
                User u = await context.Users.Include(uu => uu.Account)
                    .FirstOrDefaultAsync(uu => uu.ID == user.UserID);
                IdAvatarNamePhone i = new IdAvatarNamePhone();
                i.UserId = u.ID;
                i.Avatar = u.Avatar;
                i.Name = u.UserName;
                i.Phone = u.Account.PhoneNumber;
                list.Add(i);
            }
            return list;
        }

        private async Task<User> GetUserByRole(int eventId, int role)
        {
            EventUser eventUser = await context.EventUsers.Include(e => e.User)
                .FirstOrDefaultAsync(e => e.EventID == eventId && e.UserRole == role);
            // nếu như chưa có ai là role 2 thì sẽ lấy tạm ông role 1
            if (role == 2 && eventUser == null)
                eventUser = await context.EventUsers.Include(e => e.User)
                .FirstOrDefaultAsync(e => e.EventID == eventId && e.UserRole == 1);
            return (eventUser != null) ? eventUser.User : null;
        }
    }
}
