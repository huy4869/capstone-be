using G24_BWallet_Backend.DBContexts;
using G24_BWallet_Backend.Models;
using G24_BWallet_Backend.Models.ObjectType;
using G24_BWallet_Backend.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace G24_BWallet_Backend.Repository
{
    public class FriendRepository : IFriendRepository
    {
        private readonly MyDBContext context;

        public FriendRepository(MyDBContext myDB)
        {
            this.context = myDB;
        }

        public async Task<List<Member>> SearchFriendToInvite(int userID, string phone = null)
        {
            IQueryable<Member> list1;
            IQueryable<Member> list2;

            if (phone == null)
            {
                //list friend in friendID (mình là cột userID)
                list1 = from f in context.Friends
                        join u in context.Users.Include(u => u.Account) on f.UserFriendID equals u.ID
                        where f.UserID == userID && f.status == 1 && u.AllowInviteEventStatus == 1
                        select (new Member(u.ID, u.UserName, u.Avatar, u.Account.PhoneNumber));
                
                //list friend as userID (mình là cột UserFriendID)
                list2 = from f in context.Friends
                        join u in context.Users.Include(u => u.Account) on f.UserID equals u.ID
                        where f.UserFriendID == userID && f.status == 1 && u.AllowInviteEventStatus == 1
                        select (new Member(u.ID, u.UserName, u.Avatar, u.Account.PhoneNumber));
            }

            //tim theo phonenumber
            else
            {
                //list friend in friendID (mình là cột userID)
                list1 = from f in context.Friends
                        join u in context.Users.Include(u => u.Account) on f.UserFriendID equals u.ID
                        where f.UserID == userID
                        && u.Account.PhoneNumber.Contains(phone)
                        && f.status == 1
                        && u.AllowInviteEventStatus == 1
                        select (new Member(u.ID, u.UserName, u.Avatar, u.Account.PhoneNumber));

                //list friend in userID (mình là cột UserFriendID)
                list2 = from f in context.Friends
                        join u in context.Users.Include(u => u.Account) on f.UserID equals u.ID
                        where f.UserFriendID == userID
                        && u.Account.PhoneNumber.Contains(phone)
                        && f.status == 1
                        && u.AllowInviteEventStatus == 1
                        select (new Member(u.ID, u.UserName, u.Avatar, u.Account.PhoneNumber));
            }

            List<Member> listFriends = list1.ToList();
            listFriends.AddRange(list2);

            return listFriends.OrderBy(m => m.UserName).ToList();
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

        public async Task<List<Member>> GetFriendsAsync(int userID, string phone = null)
        {
            IQueryable<Member> list1;
            IQueryable<Member> list2;

            if (phone == null)
            {
                //list friend in friendID (mình là cột userID)
                list1 = from f in context.Friends
                        join u in context.Users.Include(u => u.Account) on f.UserFriendID equals u.ID
                        where f.UserID == userID && f.status == 1
                        select (new Member(u.ID, u.UserName, u.Avatar, u.Account.PhoneNumber));

                //list friend as userID (mình là cột UserFriendID)
                list2 = from f in context.Friends
                        join u in context.Users.Include(u => u.Account) on f.UserID equals u.ID
                        where f.UserFriendID == userID && f.status == 1
                        select (new Member(u.ID, u.UserName, u.Avatar, u.Account.PhoneNumber));
            }
            else
            {
                //list friend in friendID (mình là cột userID)
                list1 = from f in context.Friends
                        join u in context.Users.Include(u => u.Account) on f.UserFriendID equals u.ID
                        where f.UserID == userID
                        && u.Account.PhoneNumber.Contains(phone)
                        && f.status == 1
                        select (new Member(u.ID, u.UserName, u.Avatar, u.Account.PhoneNumber));

                //list friend in userID (mình là cột UserFriendID)
                list2 = from f in context.Friends
                        join u in context.Users.Include(u => u.Account) on f.UserID equals u.ID
                        where f.UserFriendID == userID
                        && u.Account.PhoneNumber.Contains(phone)
                        && f.status == 1
                        select (new Member(u.ID, u.UserName, u.Avatar, u.Account.PhoneNumber));
            }

            List<Member> listFriends = list1.ToList();
            listFriends.AddRange(list2);
            return listFriends.OrderBy(m => m.UserName).ToList();
        }

        public async Task<List<Member>> SearchFriendToAdd(int userID, string phone)
        {
            //all user can be add friend
            var ListUsers = context.Users.Include(u => u.Account)
                .Where(u => u.Account.PhoneNumber.Contains(phone))
                .Where(u => u.AllowAddFriendStatus == 1)
                .Select(u => new Member(u.ID, u.UserName, u.Avatar, u.Account.PhoneNumber))
                .ToListAsync();
            var searchResult = await ListUsers;
            var listFriend = GetFriendsAsync(userID);
            searchResult = searchResult.Except(await listFriend).ToList();
            return searchResult;
        }

        public async Task<string> SendFriendRequestAsync(int userID, int friendID)
        {
            if (userID == friendID) throw new Exception("không kết bạn được với bản thân"); 
            var user = context.Users.Where(u => u.ID == friendID).FirstOrDefault();
            if (user == null) throw new Exception("lỗi không tìm thấy người dùng");
            else if (user.AllowAddFriendStatus == 0) throw new Exception("Không kết bạn được với người này");

            var friend = context.Friends
                .Where(f => (f.UserID == userID && f.UserFriendID == friendID) 
                || (f.UserID == friendID && f.UserFriendID == userID))
                .FirstOrDefault();

            //check đã là bạn hay đã từng có request chưa
            if (friend != null){
                
                //nếu đã có lời mời của người kia 
                if (friend.UserID == friendID && friend.status == 0)
                {
                    friend.status = 1;
                    context.Friends.Update(friend);
                    await context.SaveChangesAsync();

                    return "hai bạn đã trở thành bạn";
                }
                
                //đã là bạn
                else if(friend.status == 1) return "hai bạn đã là bạn";

                //nếu mình đã gửi lời mời
                else if (friend.UserID == userID && friend.status == 0) return "đã gửi lời mời kết bạn chờ chấp thuận";
                
            }

            Friend friendRequest = new Friend();
            friendRequest.UserID = userID;
            friendRequest.UserFriendID = friendID;
            friendRequest.status = 0;
            friendRequest.CreatedAt = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
            context.Friends.Add(friendRequest);
            await context.SaveChangesAsync();

            return "đã gửi lời mời kết bạn chờ chấp thuận";
        }

        public async Task<string> AcceptFriendRequestAsync(int yourID, int userRequestID)
        {
            var friend = context.Friends.Where(f => f.UserID == userRequestID && f.UserFriendID == yourID).FirstOrDefault();
            if (friend == null)
            {
                return "lời kết bạn này không tồn tại";
            }
            friend.status = 1;
            context.Friends.Update(friend);
            await context.SaveChangesAsync();

            return "đã chấp nhận lời mời kết bạn";
        }

        public async Task<List<Member>> GetListFriendRequest(int UserID, string phone = null)
        {
            IQueryable<Member> list;
            if (phone != null)
            {
                list = from f in context.Friends
                       join u in context.Users.Include(u => u.Account) on f.UserID equals u.ID
                       where f.UserFriendID == UserID
                           && u.Account.PhoneNumber.Contains(phone)
                           && f.status == 0
                       select (new Member(u.ID, u.UserName, u.Avatar, u.Account.PhoneNumber));
            }

            //tìm theo phone
            else
            {
                list = from f in context.Friends
                join u in context.Users.Include(u => u.Account) on f.UserID equals u.ID
                where f.UserFriendID == UserID
                    && f.status == 0
                select (new Member(u.ID, u.UserName, u.Avatar, u.Account.PhoneNumber));
            }

            return list.ToList();
        }

        public async Task<string> DeleteFriendAsync(int userID, int friendID)
        {
            //check là userID và friendID đã cùng có chưa
            var friend = context.Friends
                .Where(f => (f.UserID == userID && f.UserFriendID == friendID)
                || (f.UserID == friendID && f.UserFriendID == userID))
                .FirstOrDefaultAsync();
            if (await friend != null) {
                context.Friends.Remove(await friend);
            }
            else
            {
                return "hai bạn chưa từng là bạn";
            }
            await context.SaveChangesAsync();

            return "đã xóa khỏi danh sách bạn bè";
        }
    }
}
