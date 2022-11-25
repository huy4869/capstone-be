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

        public async Task<Event> GetEvent(int eventId)
        {
            return await context.Events
                .FirstOrDefaultAsync(e => e.ID == eventId);
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
