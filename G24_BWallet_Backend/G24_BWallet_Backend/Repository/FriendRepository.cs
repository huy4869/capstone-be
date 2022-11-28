using G24_BWallet_Backend.DBContexts;
using G24_BWallet_Backend.Models;
using G24_BWallet_Backend.Models.ObjectType;
using G24_BWallet_Backend.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace G24_BWallet_Backend.Repository
{
    public class FriendRepository : IFriendRepository
    {
        private readonly MyDBContext context;

        public FriendRepository(MyDBContext myDB)
        {
            this.context = myDB;
        }

        public async Task AddInvite(EventFriendParam e)
        {
            DateTime VNDateTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
            foreach (int friendId in e.MemberIDs)
            {
                // kiểm tra xem bạn bè đã ở trong event này chưa, nếu chưa thì mới add vào
                EventUser eu = await context.EventUsers
                    .FirstOrDefaultAsync(er=>er.EventID == e.EventId && er.UserID == friendId);
                if (eu == null)// bạn bè chưa ở trong event-> tạo invite
                {
                    Invite invite = new Invite();
                    invite.UserID = e.UserId;
                    invite.FriendId = friendId;
                    invite.EventID = e.EventId;
                    invite.Status = 0;
                    invite.CreateAt = VNDateTime;
                    invite.UpdateAt = VNDateTime;
                    await context.Invites.AddAsync(invite);
                    await context.SaveChangesAsync();
                }
            }
        }

        public async Task<IQueryable<Member>> GetFriendsAsync(int userID)
        {
            var list = from u in context.Users
                       join f in context.Friends
                       on u.ID equals f.UserID
                       where u.ID == userID
                       select f;
            var list2 = from u in context.Users.Include(u => u.Account)
                        join l in list
                        on u.ID equals l.UserFriendID
                        select (new Member(u.ID, u.UserName, u.Avatar,u.Account.PhoneNumber));
            return await Task.FromResult(list2);
        }
    }
}
