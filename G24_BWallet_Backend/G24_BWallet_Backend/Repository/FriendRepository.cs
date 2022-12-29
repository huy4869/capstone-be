using G24_BWallet_Backend.DBContexts;
using G24_BWallet_Backend.Models;
using G24_BWallet_Backend.Models.ObjectType;
using G24_BWallet_Backend.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
        private readonly Format format;
        private readonly ActivityRepository activity;

        public FriendRepository(MyDBContext myDB)
        {
            this.context = myDB;
            this.format = new Format();
            this.activity = new ActivityRepository(myDB);
        }

        // show ra danh sách bạn bè chưa tham gia nhóm để mời
        public async Task<List<Member>> SearchFriendToInvite(int userID, int eventId,
            string search = null)
        {
            IQueryable<Member> list1;
            IQueryable<Member> list2;

            if (search == null)
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

            //tim theo phonenumber hoặc friend name
            else
            {
                //list friend in friendID (mình là cột userID)
                list1 = from f in context.Friends
                        join u in context.Users.Include(u => u.Account) on f.UserFriendID equals u.ID
                        where f.UserID == userID
                        && (u.Account.PhoneNumber.Contains(search) || u.UserName.Contains(search))
                        && f.status == 1
                        && u.AllowInviteEventStatus == 1
                        select (new Member(u.ID, u.UserName, u.Avatar, u.Account.PhoneNumber));

                //list friend in userID (mình là cột UserFriendID)
                list2 = from f in context.Friends
                        join u in context.Users.Include(u => u.Account) on f.UserID equals u.ID
                        where f.UserFriendID == userID
                        && (u.Account.PhoneNumber.Contains(search) || u.UserName.Contains(search))
                        && f.status == 1
                        && u.AllowInviteEventStatus == 1
                        select (new Member(u.ID, u.UserName, u.Avatar, u.Account.PhoneNumber));

            }

            List<Member> listFriends = list1.ToList();
            listFriends.AddRange(list2);
            // mình chỉ lấy những thằng friend mà chưa tham gia event hiện tại thôi
            listFriends = await GetFriendNotAttendedEvent(listFriends, eventId);
            return listFriends.OrderBy(m => m.UserName).ToList();
        }

        private async Task<List<Member>> GetFriendNotAttendedEvent(List<Member> listFriends, int eventId)
        {
            List<Member> list = new List<Member>();
            foreach (Member member in listFriends)
            {
                EventUser eventUser = await context.EventUsers
                    .FirstOrDefaultAsync(m => m.EventID == eventId && m.UserID == member.UserId && m.UserRole != 4);
                if (eventUser == null)
                    list.Add(member);
            }
            return list;
        }

        // mời bạn bè vào nhóm
        public async Task AddInvite(EventFriendParam e)
        {
            DateTime VNDateTime = TimeZoneInfo.ConvertTime(DateTime.Now,
                TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
            foreach (int friendId in e.MemberIDs)
            {
                // kiểm tra xem bạn bè đã ở trong event này chưa, nếu chưa thì mới add vào
                EventUser eu = await context.EventUsers
                    .Include(e => e.Event)
                    .FirstOrDefaultAsync(er => er.EventID == e.EventId && er.UserID == friendId && er.UserRole != 4);
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
                    await activity.InviteActivity(1, 0, e.UserId, friendId, e.EventId);
                    await activity.InviteActivity(2, 0, friendId, e.UserId, e.EventId);
                }
            }
        }

        public async Task<List<Member>> GetFriendsAsync(int userID, string search = null)
        {
            IQueryable<Member> list1;
            IQueryable<Member> list2;

            if (search == null)
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
                        && (u.Account.PhoneNumber.Contains(search) || u.UserName.Contains(search))
                        && f.status == 1
                        select (new Member(u.ID, u.UserName, u.Avatar, u.Account.PhoneNumber));

                //list friend in userID (mình là cột UserFriendID)
                list2 = from f in context.Friends
                        join u in context.Users.Include(u => u.Account) on f.UserID equals u.ID
                        where f.UserFriendID == userID
                        && (u.Account.PhoneNumber.Contains(search) || u.UserName.Contains(search))
                        && f.status == 1
                        select (new Member(u.ID, u.UserName, u.Avatar, u.Account.PhoneNumber));
            }

            List<Member> listFriends = list1.ToList();
            listFriends.AddRange(list2);
            // chỉ lấy những thằng cho phép thêm vào nhóm
            listFriends = await GetFriendAllowInviteEvent(listFriends);
            return listFriends.OrderBy(m => m.UserName).ToList();
        }

        private async Task<List<Member>> GetFriendAllowInviteEvent(List<Member> listFriends)
        {
            List<Member> list = new List<Member>();
            foreach (Member item in listFriends)
            {
                User user = await context.Users.FirstOrDefaultAsync(u =>
                u.ID == item.UserId && u.AllowInviteEventStatus == 1);
                if (user != null)
                    list.Add(item);
            }
            return list;
        }

        public async Task<List<searchFriendToAdd>> SearchFriendToAdd(int userID, string search = null)
        {
            if (search.IsNullOrEmpty())
            {
                return null;
            }
            search = format.SearchTextFormat(search);
            //all user can be add friend
            var ListUsers = context.Users.Include(u => u.Account)
                .Where(u => u.ID != userID)
                //.Where(u => u.Account.PhoneNumber.Contains(search) || format.SearchTextFormat(u.UserName).Contains(search) )
                .Select(u => new searchFriendToAdd()
                {
                    UserId = u.ID,
                    UserName = u.UserName,
                    UserAvatar = u.Avatar,
                    UserPhone = u.Account.PhoneNumber
                })
                .ToListAsync();
            List<searchFriendToAdd> searchResult = new List<searchFriendToAdd>();
            foreach (searchFriendToAdd sf in await ListUsers)
            {
                if (format.SearchTextFormat(sf.UserPhone).Contains(search) || format.SearchTextFormat(sf.UserName).Contains(search))
                    searchResult.Add(sf);
            }

            //bỏ bạn của mình
            var listFriendID = context.Friends.Where(f => f.UserID == userID && f.status == 1).Select(f => f.UserFriendID).ToList();
            listFriendID.AddRange(context.Friends.Where(f => f.UserFriendID == userID && f.status == 1).Select(f => f.UserID).ToList());

            searchResult = searchResult.Where(sr => !listFriendID.Contains(sr.UserId)).ToList();

            //foreach to change status
            foreach (searchFriendToAdd user in searchResult)
            {
                user.FriendStatus = await GetFriendStatus(userID, user.UserId);
            }

            return searchResult;
        }

        // lấy status có thể kết bạn không, đã gửi kết bạn chưa, có phải là bạn không 
        private async Task<int> GetFriendStatus(int userId, int userFriendID)
        {
            User user = await context.Users.FirstOrDefaultAsync(u => u.ID == userFriendID);
            Friend friend = await context.Friends
                .Where(f => (f.UserID == userId && f.UserFriendID == userFriendID))
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

        // gửi lời mời kết bạn
        public async Task<string> SendFriendRequestAsync(int userID, int friendID)
        {
            if (userID == friendID) throw new Exception("Không kết bạn được với bản thân!");
            var user = context.Users.Where(u => u.ID == friendID).FirstOrDefault();
            if (user == null) throw new Exception("Lỗi không tìm thấy người dùng!");
            else if (user.AllowAddFriendStatus == 0) return "Không thể kết bạn được với người này!";
            else if (CountFriend(friendID) >= 499) return "Người này đang có quá nhiều bạn bè!";

            var friend = context.Friends
                .Where(f => (f.UserID == userID && f.UserFriendID == friendID)
                || (f.UserID == friendID && f.UserFriendID == userID))
                .FirstOrDefault();

            //check đã là bạn hay đã từng có request chưa
            if (friend != null)
            {

                //nếu đã có lời mời của người kia 
                if (friend.UserID == friendID && friend.status == 0)
                {
                    friend.status = 1;
                    context.Friends.Update(friend);
                    await context.SaveChangesAsync();

                    return "Hai bạn đã trở thành bạn.";
                }

                //đã là bạn
                else if (friend.status == 1) return "Hai bạn đã là bạn.";

                //nếu mình đã gửi lời mời
                else if (friend.UserID == userID && friend.status == 0) return "Đã gửi lời mời kết bạn chờ chấp thuận.";

            }

            Friend friendRequest = new Friend();
            friendRequest.UserID = userID;
            friendRequest.UserFriendID = friendID;
            friendRequest.status = 0;
            friendRequest.CreatedAt = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
            context.Friends.Add(friendRequest);
            await context.SaveChangesAsync();
            await activity.FriendActivity(1, 0, userID, friendID);
            return "Đã gửi lời mời kết bạn chờ chấp thuận.";
        }

        // chấp thuận hoặc từ chối lời mời kết bạn
        public async Task<string> AcceptFriendRequestAsync(int yourID, Friend respone)
        {
            var friend = context.Friends.Where(f => f.UserID == respone.UserFriendID && f.UserFriendID == yourID).FirstOrDefault();
            if (friend == null)
            {
                return "Lời kết bạn này không tồn tại!";
            }
            else if (CountFriend(yourID) >= 499)
            {
                return "Số bạn bè của bạn đang vượt quá giới hạn!";
            }
            else if (respone.status == 0)
            {
                await activity.FriendActivity(2, 0, yourID, respone.UserFriendID);
                await activity.FriendActivity(3, 0, respone.UserFriendID, yourID);
                context.Friends.Remove(friend);
                await context.SaveChangesAsync();

                return "Đã từ chối lời mời kết bạn.";
            }

            friend.status = 1;
            await activity.FriendActivity(2, 1, yourID, respone.UserFriendID);
            await activity.FriendActivity(3, 1, respone.UserFriendID, yourID);
            context.Friends.Update(friend);
            await context.SaveChangesAsync();

            return "Đã chấp nhận lời mời kết bạn.";
        }

        public async Task<List<Member>> GetListFriendRequest(int UserID, string search = null)
        {
            IQueryable<Member> list;
            if (search != null)
            {
                list = from f in context.Friends
                       join u in context.Users.Include(u => u.Account) on f.UserID equals u.ID
                       where f.UserFriendID == UserID
                           && (u.Account.PhoneNumber.Contains(search) || u.UserName.Contains(search))
                           && f.status == 0
                       select (new Member(u.ID, u.UserName, u.Avatar, u.Account.PhoneNumber));
            }

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
            if (await friend != null)
            {
                await activity.FriendActivity(4, 0, userID, friendID);
                context.Friends.Remove(await friend);
            }
            await context.SaveChangesAsync();

            return "Xóa khỏi danh sách bạn bè thành công.";
        }

        public int CountFriend(int userID)
        {
            return context.Friends.
                Where(f => (f.UserID == userID || f.UserFriendID == userID)
                && f.status == 1)
                .Distinct().Count();
        }
    }
}
