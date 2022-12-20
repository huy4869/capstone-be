using G24_BWallet_Backend.DBContexts;
using G24_BWallet_Backend.Models;
using G24_BWallet_Backend.Models.ObjectType;
using G24_BWallet_Backend.Repository;
using G24_BWallet_Backend.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace G24_BWallet_Backend.Repository
{
    public class PaidDebtRepository : IPaidDebtRepository
    {
        private readonly MyDBContext context;
        private readonly ActivityRepository activity;
        private readonly Format format;
        private readonly IMemberRepository memberRepository;
        private readonly IEventRepository eventRepository;
        public PaidDebtRepository(MyDBContext myDB, IMemberRepository memberRepository,
            IEventRepository eventRepository)
        {
            this.context = myDB;
            activity = new ActivityRepository(myDB);
            format = new Format();
            this.memberRepository = memberRepository;
            this.eventRepository = eventRepository;
        }
        public async Task<List<Receipt>> GetReceipts(int eventId, int status)
        {
            var list = context.Receipts.Include(r => r.UserDepts).Include(r => r.User)
                .Where(r => r.EventID == eventId && (r.ReceiptStatus == status || r.ReceiptStatus == 4))
                .OrderByDescending(r => r.Id)
                .ToListAsync();
            return await list;
        }

        public async Task<List<UserDebtReturn>> GetUserDepts(List<Receipt> receipt, int userId)
        {
            List<UserDebtReturn> userDepts = new List<UserDebtReturn>();
            foreach (var item in receipt)
            {
                UserDept ud = item.UserDepts
                    .Where(ud => ud.UserId == userId && (ud.DeptStatus == 2 || ud.DeptStatus == 4)
                && ud.DebtLeft > 0)
                    .FirstOrDefault();
                if (ud != null)
                {
                    UserDebtReturn udr = new UserDebtReturn();
                    udr.UserDeptId = ud.Id;
                    udr.ReceiptName = item.ReceiptName;
                    udr.Date = item.CreatedAt + "";
                    udr.OwnerName = item.User.UserName;
                    udr.DebtLeft = ud.DebtLeft;
                    udr.DebtLeftFormat = format.MoneyFormat(ud.DebtLeft);
                    udr.status = ud.DeptStatus;
                    userDepts.Add(udr);
                }

            }
            userDepts.Reverse();
            return await Task.FromResult(userDepts);
        }

        // tạo yêu cầu trả tiền
        public async Task<PaidDept> PaidDebtInEvent(PaidDebtParam p)//create paid dept
        {
            DateTime VNDateTimeNow = TimeZoneInfo
                .ConvertTime(DateTime.Now,
                TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
            int status = 1;
            // kiểm tra nếu mình là owner hoặc cashier thì duyệt luôn, ko phải chờ
            if (await IsOwner(p.EventId, p.UserId) || await IsCashier(p.EventId, p.UserId))
                status = 2;
            PaidDept paidDept = new PaidDept
            {
                UserId = p.UserId,
                EventId = p.EventId,
                TotalMoney = p.TotalMoney,
                Status = status,
                Code = p.Code,
                Type = p.Type,
                UpdatedAt = VNDateTimeNow,
                CreatedAt = VNDateTimeNow
            };
            //try
            //{
            await context.PaidDepts.AddAsync(paidDept);
            await context.SaveChangesAsync();
            foreach (var item in p.ListEachPaidDebt)
            {
                var check = paidDept.Id;
                PaidDebtList paid = new PaidDebtList
                {
                    PaidId = paidDept.Id,
                    DebtId = item.userDeptId,
                    PaidAmount = item.debtLeft
                };
                await context.PaidDebtLists.AddAsync(paid);
                await context.SaveChangesAsync();

            }
            // nếu được duyệt thì mới trừ tiền luôn, còn không thì phải chờ duyệt mới trừ
            if (status == 2)
                await ChangeDebtLeft(paidDept);
            await activity.CreatorPaidDebtActivity(p.UserId, p.TotalMoney, p.EventId);
            //}
            //catch (Exception e)
            //{
            //    throw new Exception("PaidDept:Lỗi ghi tiền trả");
            //}
            return paidDept;
        }


        // sau khi accept yêu cầu trả tiền thì tiến hành trừ tiền
        private async Task ChangeDebtLeft(PaidDept paidDept)
        {
            List<PaidDebtList> paidDebtLists = await context.PaidDebtLists
                .Where(p => p.PaidId == paidDept.Id).ToListAsync();
            foreach (PaidDebtList item in paidDebtLists)
            {
                UserDept userDebt = await context.UserDepts
                    .FirstOrDefaultAsync(u => u.Id == item.DebtId);
                userDebt.DebtLeft -= item.PaidAmount;
                if (userDebt.DebtLeft <= 0)// trả hết nợ
                {
                    userDebt.DeptStatus = 0;
                }
                await context.SaveChangesAsync();
                // kiểm tra nếu tất cả userdept có chung 1 receipt id mà trả hết rồi(status =0)
                // thì chuyển status của receipt đó thành đã trả hết(cũng = 0 luôn)
                await ChangeReceiptStatus(userDebt.ReceiptId);
            }
            // sau khi cập nhật xong xuôi hết thì mình cần kiểm tra xem tổng mình nợ 
            // và tổng phải thu của mình trong event nếu bằng nhau, thì cho tất cả 
            // receipt mình tạo(đuwong nhiên là có cả các userdept của nó)
            // và userdept của mình bằng 0 (đã thu và đã trả hêt)
            await CheckReceiptUserDebt(paidDept);
        }


        // sau khi cập nhật xong xuôi hết thì mình cần kiểm tra xem tổng mình nợ 
        // và tổng phải thu của mình trong event nếu bằng nhau, thì cho tất cả 
        // receipt mình tạo(đương nhiên là có cả các userdept của nó)
        // và userdept của mình bằng 0 (đã thu và đã trả hêt)
        private async Task CheckReceiptUserDebt(PaidDept paidDept)
        {
            // đầu tiên là lấy tổng tiền mình nợ
            double totalDebt = (await eventRepository
                .GetDebtMoney(paidDept.EventId, paidDept.UserId)).Money.Amount;
            // xong là lấy tổng tiền mình cần nhận lại
            double totalReceive = (await eventRepository
                .GetReceiveMoney(paidDept.EventId, paidDept.UserId)).Money.Amount;
            // nếu 2 thằng trên mà bằng nhau thì cho hết những thứ liên quan thành đã trả hết
            if (totalDebt != totalReceive)
                return;
            // đầu tiên là duyệt tất cả các receipt trong event, cái nào mình tạo thì cho thành đã trả
            // hết(cả userdept trong đấy), cái nào mình nợ thì cái userdept đấy thành đã trả hết
            List<Receipt> receipts = await context.Receipts.Include(r => r.UserDepts)
                .Where(r => r.EventID == paidDept.EventId && (r.ReceiptStatus == 2 || r.ReceiptStatus == 4))
                .ToListAsync();
            foreach (Receipt receipt in receipts)
            {
                // nếu là mình tạo
                if (receipt.UserID == paidDept.UserId)
                {
                    receipt.ReceiptStatus = 0;
                    await context.SaveChangesAsync();
                    List<UserDept> userDepts = receipt.UserDepts;
                    foreach (UserDept userDept in userDepts)
                    {
                        userDept.DeptStatus = 0;
                        await context.SaveChangesAsync();
                    }
                    continue;
                }
                // còn nếu receipt này ko phải mình tạo thì check xem mình có nợ không, nếu
                // mình có nợ thì cái userdept đấy thành đã trả hết luôn
                List<UserDept> userDeptss = receipt.UserDepts;
                foreach (UserDept userDept in userDeptss)
                {
                    if (userDept.UserId == paidDept.UserId)
                    {
                        userDept.DeptStatus = 0;
                        await context.SaveChangesAsync();
                        break;
                    }
                }
            }
            // cuối cùng là kiểm tra trong event này, tất cả các user dept của cái nào mà trả hết rồi
            // thì chuyển receipt status thành 0
            await ChangeAllReceiptStatusInEvent(paidDept.EventId);
        }

        // kiểm tra trong event này, tất cả các user dept của cái nào mà trả hết rồi
        // thì chuyển receipt status thành 0
        private async Task ChangeAllReceiptStatusInEvent(int eventId)
        {
            List<Receipt> receipts = await context.Receipts.Include(r => r.UserDepts)
                .Where(r => r.EventID == eventId && (r.ReceiptStatus == 2 || r.ReceiptStatus == 4))
                .ToListAsync();
            foreach (Receipt receipt in receipts)
            {
                bool allUserDeptDone = true;
                // duyệt hết userdept của receipt này
                List<UserDept> userDepts = receipt.UserDepts;
                foreach (UserDept userDept in userDepts)
                {
                    if (userDept.DeptStatus == 2 || userDept.DeptStatus == 4)
                    {
                        allUserDeptDone = false;
                        break;
                    }

                }
                // nếu biến kia là true nghĩa là tất cả các userdept đã trả hết rồi=> receipt = 0
                if (allUserDeptDone)
                {
                    receipt.ReceiptStatus = 0;
                    await context.SaveChangesAsync();
                }
            }
        }

        // kiểm tra nếu tất cả userdept có chung 1 receipt id mà trả hết rồi(status =0)
        // thì chuyển status của receipt đó thành đã trả hết(cũng = 0 luôn)
        private async Task ChangeReceiptStatus(int receiptId)
        {
            List<UserDept> userDepts = await context.UserDepts.Include(u => u.Receipt)
                .Where(u => u.ReceiptId == receiptId).ToListAsync();
            foreach (UserDept item in userDepts)
            {
                // nếu 1 userdept có deft left > 0 hoặc status != 0
                // thì return luôn vì chưa trả hết
                if (item.DeptStatus != 0 || item.DebtLeft > 0)
                    return;
            }
            // nếu xuống đc đây thì có nghĩa là hoá đơn này đã trả hết
            Receipt receipt = await context.Receipts
                .FirstOrDefaultAsync(r => r.Id == receiptId);
            receipt.ReceiptStatus = 0;
            await context.SaveChangesAsync();
        }

        // danh sách các yêu cầu trả tiền chờ duyệt hoặc đã duyệt
        public async Task<List<DebtPaymentPending>> PaidsWaitingOrHandled(int userId,
            int eventId, bool isWaiting)
        {
            List<DebtPaymentPending> result = new List<DebtPaymentPending>();
            // lấy hết paiddebt trong event này
            List<PaidDept> paidDepts = await context.PaidDepts
                .Include(p => p.User)
                .OrderByDescending(p => p.UpdatedAt)
                .Where(p => p.EventId == eventId).ToListAsync();
            User cashier = await GetCashier(eventId);

            foreach (PaidDept item in paidDepts)
            {
                DebtPaymentPending debtPayment = new DebtPaymentPending();
                // nếu là cashier hoặc owner thì user hiện ra sẽ là người tạo paidDebt
                // nếu không thì sẽ hiện ông cashier
                if (cashier.ID == userId || await IsOwner(eventId, userId))
                {
                    debtPayment.User = new UserAvatarName
                    {
                        Avatar = item.User.Avatar,
                        Name = item.User.UserName,
                        Phone = await memberRepository.GetPhoneByUserId(item.User.ID)
                    };
                    // nếu mình là cashier or owner thì chỉ xem những paid đang chờ duyệt
                    // nếu trạng thái hiện tại là xem những paid đang chờ duyệt
                    if (item.Status != 1 && isWaiting == true)
                        continue;
                    // ngược lại, nếu trạng thái là xem lịch sử các yêu cầu đã duyệt
                    // thì sẽ bỏ qua các status == 1
                    if (item.Status == 1 && isWaiting == false)
                        continue;
                }
                else
                    debtPayment.User = new UserAvatarName
                    {
                        Avatar = cashier.Avatar,
                        Name = cashier.UserName,
                        Phone = await memberRepository.GetPhoneByUserId(cashier.ID)
                    };
                debtPayment.PaidDebtId = item.Id;
                debtPayment.TotalMoney = item.TotalMoney;
                debtPayment.TotalMoneyFormat = format.MoneyFormat(item.TotalMoney);
                debtPayment.Date = item.CreatedAt.ToString();
                debtPayment.Code = item.Code;
                debtPayment.ImageLink = await context.ProofImages
                    .Where(p => p.ImageType.Equals("paidDept") && p.ModelId == item.Id)
                    .Select(p => p.ImageLink).FirstOrDefaultAsync();
                if (item.Type.Equals("money"))
                    debtPayment.Type = "Tiền mặt";
                else
                    debtPayment.Type = "Chuyển khoản";
                debtPayment.Status = item.Status;
                result.Add(debtPayment);
            }
            return result;
        }

        // danh sách các yêu cầu trả tiền mình đã gửi trong event này, xem car 3 trạng thái
        // duyệt,đang chờ, bị từ chối
        public async Task<List<DebtPaymentPending>> DebtSent(int userId, int eventId)
        {
            List<DebtPaymentPending> result = new List<DebtPaymentPending>();
            // lấy hết paiddebt của mình trong event này
            List<PaidDept> paidDepts = await context.PaidDepts
                .Include(p => p.User)
                .OrderByDescending(p => p.UpdatedAt)
                .Where(p => p.EventId == eventId && p.UserId == userId).ToListAsync();
            User cashier = await GetCashier(eventId);
            foreach (PaidDept item in paidDepts)
            {
                DebtPaymentPending debtPayment = new DebtPaymentPending();
                // nếu là cashier hoặc owner thì user hiện ra sẽ là người tạo paidDebt
                // nếu không thì sẽ hiện ông cashier
                if (cashier.ID == userId || await IsOwner(eventId, userId))
                {
                    debtPayment.User = new UserAvatarName
                    { Avatar = item.User.Avatar, Name = item.User.UserName };
                }
                else
                    debtPayment.User = new UserAvatarName
                    { Avatar = cashier.Avatar, Name = cashier.UserName };
                debtPayment.PaidDebtId = item.Id;
                debtPayment.TotalMoney = item.TotalMoney;
                debtPayment.TotalMoneyFormat = format.MoneyFormat(item.TotalMoney);
                debtPayment.Date = item.CreatedAt.ToString();
                debtPayment.Code = item.Code;
                debtPayment.ImageLink = await context.ProofImages
                    .Where(p => p.ImageType.Equals("paidDept") && p.ModelId == item.Id)
                    .Select(p => p.ImageLink).FirstOrDefaultAsync();
                if (item.Type.Equals("money"))
                    debtPayment.Type = "Tiền mặt";
                else
                    debtPayment.Type = "Chuyển khoản";
                debtPayment.Status = item.Status;
                result.Add(debtPayment);
            }
            return result;
        }

        private async Task<User> GetCashier(int eventId)
        {
            EventUser cashier = await context.EventUsers.Include(e => e.User)
                .FirstOrDefaultAsync(u => u.EventID == eventId && u.UserRole == 3);
            EventUser owner = await context.EventUsers.Include(e => e.User)
                .FirstOrDefaultAsync(u => u.EventID == eventId && u.UserRole == 1);
            EventUser inspector = await context.EventUsers.Include(e => e.User)
                .FirstOrDefaultAsync(u => u.EventID == eventId && u.UserRole == 2);
            if (cashier != null) return cashier.User;
            else if (owner != null) return owner.User;
            return inspector.User;
        }


        public async Task<List<DebtPaymentPending>> PaidWaitConfirm(int eventId)
        {
            List<DebtPaymentPending> result = new List<DebtPaymentPending>();
            List<PaidDept> paidDepts = await context.PaidDepts
                .Include(p => p.User)
                .OrderByDescending(p => p.Id)
                .Where(p => p.EventId == eventId && p.Status == 1).ToListAsync();
            foreach (PaidDept item in paidDepts)
            {
                DebtPaymentPending debt = new DebtPaymentPending();
                debt.PaidDebtId = item.Id;
                debt.Status = item.Status;
                debt.TotalMoney = item.TotalMoney;
                debt.Date = item.CreatedAt.ToString();
                debt.Code = item.Code;
                debt.ImageLink = await context.ProofImages
                    .Where(p => p.ImageType.Equals("paidDept") && p.ModelId == item.Id)
                    .Select(p => p.ImageLink).FirstOrDefaultAsync();
                debt.Type = item.Type;
                debt.User = new UserAvatarName
                {
                    Avatar = item.User.Avatar,
                    Name = item.User.UserName
                };
                result.Add(debt);
            }

            return result;
        }

        // duyệt hoặc từ chối các yêu cầu trả tiền
        public async Task PaidDebtApprove(ListIdStatus paid, int userId)
        {
            foreach (int paidid in paid.ListId)
            {
                PaidDept paidDept = await context.PaidDepts.FirstOrDefaultAsync(p => p.Id == paidid);
                paidDept.Status = paid.Status;
                paidDept.UpdatedAt = DateTime.Now;
                if (paid.Status == 2)// duyệt
                {
                    await ChangeDebtLeft(paidDept);
                }
                await activity.CreatorPaidDebtApprovedActivity(paidid, paidDept.UserId, paid.Status);
                await activity.InspectorPaidDebtApprovedActivity(paidid, userId, paid.Status);
                await context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsOwner(int eventId, int userId)
        {
            EventUser eu = await context.EventUsers
                .FirstOrDefaultAsync(ee => ee.EventID == eventId && ee.UserID == userId);
            return eu.UserRole == 1;
        }

        public async Task<bool> IsCashier(int eventId, int userId)
        {
            EventUser eu = await context.EventUsers
                .FirstOrDefaultAsync(ee => ee.EventID == eventId && ee.UserID == userId);
            if (eu.UserRole == 3) return true;
            else if (eu.UserRole == 1) return true;
            return false;
        }

        // xem chi tiết yêu cầu trả tiền khi click vào
        public async Task<PaidDebtDetailScreen> PaidDebtDetail(int paidid)
        {
            PaidDebtDetailScreen paid = new PaidDebtDetailScreen();
            PaidDept paidDept = await context.PaidDepts
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == paidid);
            paid.Code = paidDept.Code;
            paid.Date = paidDept.CreatedAt;
            paid.ImgLink = await context.ProofImages
                    .Where(p => p.ImageType.Equals("paidDept") && p.ModelId == paidid)
                    .Select(p => p.ImageLink).FirstOrDefaultAsync();
            List<UserAvatarNameMoney> userList = new List<UserAvatarNameMoney>();
            // lấy ra tổng tiền mình trả, add luôn vào đầu danh sách
            UserAvatarNameMoney current = new UserAvatarNameMoney();
            current.Avatar = paidDept.User.Avatar;
            current.Name = paidDept.User.UserName;
            current.Phone = await GetPhoneByUserId(paidDept.UserId);
            current.Role = await memberRepository.GetRole(paidDept.EventId, paidDept.UserId);
            current.TotalAmount = paidDept.TotalMoney;
            current.TotalAmountFormat = format.MoneyFormat(paidDept.TotalMoney);
            userList.Add(current);

            // lấy ra tất cả các thằng chủ nợ mà mình trả tiền
            List<PaidDebtList> paidDebtLists = await context.PaidDebtLists
                .Include(p => p.UserDept)
                .OrderByDescending(p => p.Id)
                .Where(p => p.PaidId == paidid).ToListAsync();
            foreach (PaidDebtList item in paidDebtLists)
            {
                UserDept userDept = item.UserDept;
                // từ userdept lấy ra receipt id, xong mới suy ra id thằng chủ nợ
                int ownerId = (await context.Receipts
                    .FirstOrDefaultAsync(r => r.Id == userDept.ReceiptId)).UserID;
                // đây là thằng chủ nợ
                User ownerDebt = await context.Users
                    .FirstOrDefaultAsync(u => u.ID == ownerId);
                // add vào user list
                UserAvatarNameMoney owner = new UserAvatarNameMoney();
                owner.Avatar = ownerDebt.Avatar;
                owner.Name = ownerDebt.UserName;
                owner.Phone = await GetPhoneByUserId(ownerDebt.ID);
                owner.Role = await memberRepository.GetRole(paidDept.EventId, ownerDebt.ID);
                owner.TotalAmount = item.PaidAmount;
                owner.TotalAmountFormat = format.MoneyFormat(item.PaidAmount);
                // Kiểm tra trong list có ông này chưa, nếu chưa có mới add
                // có rồi thì chỉ cộng thêm tiền thôi
                if (await PhoneExist(userList, owner.Phone))

                    await UpdateMoney(userList, owner.Phone, owner.TotalAmount);
                else
                    userList.Add(owner);
            }
            paid.Users = userList;
            return paid;
        }

        private async Task UpdateMoney(List<UserAvatarNameMoney> userList,
            string phone, double totalAmount)
        {
            foreach (var item in userList)
            {
                if (item.Phone.Equals(phone))
                {
                    item.TotalAmount = item.TotalAmount + totalAmount;
                    item.TotalAmountFormat = format.MoneyFormat(item.TotalAmount);
                    return;
                }
            }
        }

        private async Task<bool> PhoneExist(List<UserAvatarNameMoney> userList, string phone)
        {
            foreach (var item in userList)
            {
                if (item.Phone.Equals(phone))
                    return true;
            }
            return false;
        }

        private async Task<string> GetPhoneByUserId(int useriD)
        {
            User user = await context.Users.Include(u => u.Account)
                .FirstOrDefaultAsync(u => u.ID == useriD);
            return user.Account.PhoneNumber;
        }

        // kiểm tra yêu cầu trả tiền trước đấy đã duyệt chưa, nếu chưa thì không được tạo thêm 
        // yêu cầu trả tiền
        public async Task<bool> PaidCheck(int eventId, int userId)
        {
            PaidDept paid = await context.PaidDepts
                .FirstOrDefaultAsync(p => p.EventId == eventId && p.UserId == userId
                && p.Status == 1);
            return paid == null;
        }

        public async Task<bool> IsCodeExist(string code)
        {
            PaidDept paidDept = await context.PaidDepts
                .FirstOrDefaultAsync(p => p.Equals(code));
            return (paidDept != null);
        }
    }
}
