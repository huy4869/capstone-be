﻿using Amazon.S3.Transfer;
using Amazon.S3;
using G24_BWallet_Backend.DBContexts;
using G24_BWallet_Backend.Models;
using G24_BWallet_Backend.Models.ObjectType;
using G24_BWallet_Backend.Repository.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Amazon;
using Microsoft.Extensions.Logging;
using Twilio.TwiML.Fax;
using System.Data;

namespace G24_BWallet_Backend.Repository
{
    public class ReceiptRepository : IReceiptRepository
    {
        private readonly MyDBContext myDB;
        private readonly IConfiguration _configuration;
        private readonly Format format;
        private readonly ActivityRepository activity;
        private readonly IMemberRepository memberRepository;

        public ReceiptRepository(MyDBContext myDB, IConfiguration _configuration,
            IMemberRepository memberRepository)
        {
            this.myDB = myDB;
            this._configuration = _configuration;
            this.format = new Format();
            activity = new ActivityRepository(myDB);
            this.memberRepository = memberRepository;
        }

        public async Task<Receipt> AddReceiptAsync(ReceiptCreateParam addReceipt, int userRole)//
        {
            Receipt storeReceipt = new Receipt();
            storeReceipt.EventID = addReceipt.EventID;
            storeReceipt.UserID = addReceipt.UserID;
            storeReceipt.ReceiptName = addReceipt.ReceiptName;
            storeReceipt.ReceiptAmount = addReceipt.ReceiptAmount;
            storeReceipt.ReceiptStatus = (userRole == 1 || userRole == 2) ? 2 : 1;

            DateTime VNDateTimeNow = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
            storeReceipt.CreatedAt = VNDateTimeNow;
            storeReceipt.UpdatedAt = VNDateTimeNow;

            await myDB.Receipts.AddAsync(storeReceipt);
            await myDB.SaveChangesAsync();
            await activity.AddReceiptActivity(addReceipt.UserID, addReceipt.ReceiptName, addReceipt.EventID);
            return storeReceipt;
        }

        public async Task<ReceiptDetail> GetReceiptByIDAsync(int ReceiptID)
        {

            ReceiptDetail r = myDB.Receipts.Where(r => r.Id == ReceiptID).Include(r => r.User)
                .Select(r => new ReceiptDetail
                {
                    Id = r.Id,
                    OwnerId = r.UserID,
                    OwnerName = r.User.UserName,//tên chủ nợ
                    ReceiptName = r.ReceiptName,
                    ReceiptStatus = r.ReceiptStatus,
                    ReceiptAmount = r.ReceiptAmount,
                    ReceiptAmountFormat = format.MoneyFormat(r.ReceiptAmount),
                    CreatedAt = r.CreatedAt
                }).FirstOrDefault();

            //lấy ảnh receipt 
            var listIMG = myDB.ProofImages
                .Where(pf => pf.ImageType == "receipt")
                .Where(pf => pf.ModelId == ReceiptID)
                .Select(pf => pf.ImageLink)
                .ToList();

            //lấy list UserDept
            var listUDept = myDB.UserDepts.Where(ud => ud.ReceiptId == ReceiptID).Include(r => r.User)
                .Select(ud => new ReceiptDetailDept
                {
                    DeptId = ud.Id,
                    UserId = ud.UserId,
                    UserName = ud.User.UserName,//tên người nợ 
                    Avatar = ud.User.Avatar,
                    DebtLeft = ud.DebtLeft,
                    DebtLeftFormat = format.MoneyFormat(ud.DebtLeft)
                }).ToListAsync();

            r.IMGLinks = listIMG;
            r.ListUserDepts = await listUDept;
            return r;
        }

        // lấy danh sách các chứng từ khi bấm vào 1 event từ màn main
        public async Task<EventReceiptsInfo> GetEventReceiptsInfoAsync(int EventID, int userID)
        {
            EventReceiptsInfo eventInfo = myDB.Events
                .Where(e => e.ID == EventID)
                .Select(e => new EventReceiptsInfo
                {
                    Id = e.ID,
                    EventName = e.EventName,
                    EventLogo = e.EventLogo,
                    EventStatus = e.EventStatus
                    //TotalReceiptsAmount = 0
                })
                .FirstOrDefault();
            if (eventInfo == null) return null;


            List<ReceiptMainInfo> listReceipt = myDB.Receipts
                .Where(r => r.EventID == EventID)
                .Where(r => r.ReceiptStatus == 2 || r.ReceiptStatus == 4 || r.ReceiptStatus == 0)
                .OrderByDescending(r => r.Id)
                .Select(r => new ReceiptMainInfo
                {
                    Id = r.Id,
                    ReceiptName = r.ReceiptName,
                    ReceiptAmount = r.ReceiptAmount,
                    ReceiptAmountFormat = format.MoneyFormat(r.ReceiptAmount),
                    ReceiptStatus = r.ReceiptStatus,
                    CreatedAt = format.DateFormat(r.CreatedAt)
                })
                .ToList();
            //eventInfo.TotalReceiptsAmount = listReceipt.Sum(r => r.ReceiptAmount);
            NumberMoney Debt = await GetDebtMoney(EventID, userID);
            NumberMoney Receive = await GetReceiveMoney(EventID, userID);
            //// sau khi biết số tiền mình nợ event và số tiền mình cần nhận lại thì tính chung ra 1 cái
            //eventInfo.ReceiveOrPaidAmount = await ReceiveOrPaidAmount(Debt, Receive);
            eventInfo.DebtAmountFormat = format.MoneyFormat(Debt.Money.Amount);
            eventInfo.ReceiveAmountFormat = format.MoneyFormat(Receive.Money.Amount);
            eventInfo.UserAmountFormat = format.MoneyFormat(await AllUserMoneyInEvent(EventID, userID));
            eventInfo.GroupAmountFormat = format.MoneyFormat(await AllGroupMoneyInEvent(EventID, userID));
            eventInfo.TotalAmountFormat = format.MoneyFormat(
                await AllUserMoneyInEvent(EventID, userID) +
                await AllGroupMoneyInEvent(EventID, userID)
                );
            eventInfo.UserAmount = await AllUserMoneyInEvent(EventID, userID);
            eventInfo.GroupAmount = await AllGroupMoneyInEvent(EventID, userID);
            eventInfo.TotalAmount =
                await AllUserMoneyInEvent(EventID, userID) +
                await AllGroupMoneyInEvent(EventID, userID);
            eventInfo.Number = await GetNumberNotify(EventID, userID);
            eventInfo.listReceipt = listReceipt;

            return eventInfo;
        }

        private async Task<MoneyColor> ReceiveOrPaidAmount(NumberMoney debt, NumberMoney receive)
        {
            MoneyColor moneyColor = new MoneyColor();
            if (debt.Money.Amount > receive.Money.Amount)
            {
                moneyColor.Amount = debt.Money.Amount - receive.Money.Amount;
                moneyColor.AmountFormat = format.MoneyFormat(moneyColor.Amount);
                moneyColor.Color = "Red";
            }
            else if (debt.Money.Amount < receive.Money.Amount)
            {
                moneyColor.Amount = receive.Money.Amount - debt.Money.Amount;
                moneyColor.AmountFormat = format.MoneyFormat(moneyColor.Amount);
                moneyColor.Color = "Green";
            }
            else if (debt.Money.Amount == receive.Money.Amount)
            {
                moneyColor.Amount = 0;
                moneyColor.AmountFormat = format.MoneyFormat(moneyColor.Amount);
                moneyColor.Color = "Gray";
            }
            return moneyColor;
        }

        private async Task<int> GetNumberNotify(int eventID, int userID)
        {
            // số lượng yêu cầu tham gia event này: owner
            int requestNum = (await myDB.Requests
                .Where(r => r.EventID == eventID && r.Status == 3).ToListAsync()).Count;
            // số lượng chứng từ chờ duyệt : inspector
            int receiptNum = (await myDB.Receipts
                .Where(r => r.EventID == eventID && r.ReceiptStatus == 1).ToListAsync()).Count;
            // số lượng yêu cầu trả tiền chờ duyêt : cashier
            int paidNum = (await myDB.PaidDepts
                .Where(p => p.EventId == eventID && p.Status == 1).ToListAsync()).Count;
            // số lượng báo cáo chờ duyệt status == 0
            int reportNum = (await myDB.Reports
                .Where(r => r.EventId == eventID && r.ReportStatus == 0).ToListAsync()).Count;
            // kiểm tra role user
            if (await IsOwner(eventID, userID))
            {
                return requestNum + receiptNum + paidNum + reportNum;
            }
            if (await IsInspector(eventID, userID))
            {
                return receiptNum;
            }
            if (await IsCashier(eventID, userID))
            {
                return paidNum;
            }
            return 0;
        }

        //số tiền mà nhóm đa chi trong event
        private async Task<double> AllGroupMoneyInEvent(int eventID, int userID)
        {
            List<Receipt> receipts = await myDB.Receipts
               .Where(r => r.EventID == eventID && r.UserID != userID
               && (r.ReceiptStatus == 2 || r.ReceiptStatus == 0 || r.ReceiptStatus == 4)).ToListAsync();
            double amount = 0;
            receipts.ForEach(r => amount += r.ReceiptAmount);
            return amount;
        }

        // số tiền mà mình đã chi trrong event
        private async Task<double> AllUserMoneyInEvent(int eventID, int userID)
        {
            List<Receipt> receipts = await myDB.Receipts
                .Where(r => r.EventID == eventID && r.UserID == userID
                && (r.ReceiptStatus == 2 || r.ReceiptStatus == 0 || r.ReceiptStatus == 4)).ToListAsync();
            double amount = 0;
            receipts.ForEach(r => amount += r.ReceiptAmount);
            return amount;
        }

        private async Task<NumberMoney> GetDebtMoney(int eventId, int userID)
        {
            NumberMoney number = new NumberMoney();
            MoneyColor moneyColor = new MoneyColor();
            double mon = 0;
            int total = 0;
            List<int> userIdList = new List<int>();
            // lấy hết các receipt đang trả trong event này
            // lấy hết các receipt đang trả trong event này
            List<Receipt> receiptList = await myDB.Receipts
                .Where(r => r.EventID == eventId && (r.ReceiptStatus == 2 || r.ReceiptStatus == 4))
                .ToListAsync();
            foreach (Receipt receipt in receiptList)
            {
                UserDept userDept = await myDB.UserDepts
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

        private async Task<NumberMoney> GetReceiveMoney(int eventId, int userID)
        {
            NumberMoney number = new NumberMoney();
            MoneyColor moneyColor = new MoneyColor();
            double mon = 0;
            int total = 0;
            List<int> userIdList = new List<int>();
            // lấy hết các receipt mình tạo trong event này mà vẫn đang trả
            List<Receipt> receiptList = await myDB.Receipts
                .Where(r => r.EventID == eventId && (r.ReceiptStatus == 2 || r.ReceiptStatus == 4)
                && r.UserID == userID).ToListAsync();
            foreach (Receipt receipt in receiptList)
            {
                List<UserDept> userDepts = await myDB.UserDepts
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
        //public async Task<MoneyColor> GetTotalMoney(NumberMoney debt, NumberMoney receive)
        //{
        //    MoneyColor money = new MoneyColor();
        //    if (debt.Money.Amount > receive.Money.Amount)
        //    {
        //        money.Color = "Red";
        //        money.Amount = debt.Money.Amount - receive.Money.Amount;
        //        money.AmountFormat = format.MoneyFormat(money.Amount);
        //    }
        //    else if (debt.Money.Amount < receive.Money.Amount)
        //    {
        //        money.Color = "Green";
        //        money.Amount = receive.Money.Amount - debt.Money.Amount;
        //        money.AmountFormat = format.MoneyFormat(money.Amount);
        //    }
        //    else if (debt.Money.Amount == receive.Money.Amount)
        //    {
        //        money.Color = "Gray";
        //        money.Amount = receive.Money.Amount - debt.Money.Amount;
        //        money.AmountFormat = format.MoneyFormat(money.Amount);
        //    }
        //    return await Task.FromResult(money);
        //}


        //show thông tin chi tiết của receipt khi click vào cái số tiền
        public async Task<ReceiptUserDeptName> GetReceiptDetail(int receiptId)
        {
            ReceiptUserDeptName result = new ReceiptUserDeptName();
            Receipt receipt = await myDB.Receipts.Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == receiptId);
            result.ReceiptName = receipt.ReceiptName;
            result.Date = format.DateFormat(receipt.CreatedAt);
            // chỗ này phải lấy thằng tạo receipt chứ không phải thằng cashier
            //User cashier = await GetCashier(receipt.EventID);
            User creator = receipt.User;
            result.User = new UserAvatarNameMoney
            {
                Avatar = creator.Avatar,
                Name = creator.UserName,
                Phone = await memberRepository.GetPhoneByUserId(creator.ID),
                Role = await memberRepository.GetRole(receipt.EventID, creator.ID),
                TotalAmount = receipt.ReceiptAmount,
                TotalAmountFormat = format.MoneyFormat(receipt.ReceiptAmount)
            };
            double totalDebt = 0;
            List<UserAvatarNameMoney> userDepts = new List<UserAvatarNameMoney>();
            List<UserDept> depts = await myDB.UserDepts.Include(r => r.User)
                .Where(r => r.ReceiptId == receiptId).ToListAsync();
            foreach (UserDept item in depts)
            {
                UserAvatarNameMoney user = new UserAvatarNameMoney();
                user.Avatar = item.User.Avatar;
                user.Name = item.User.UserName;
                user.Phone = await memberRepository.GetPhoneByUserId(item.UserId);
                user.Role = await memberRepository.GetRole(receipt.EventID, item.UserId);
                user.TotalAmount = item.Debt;
                user.TotalAmountFormat = format.MoneyFormat(item.Debt);
                userDepts.Add(user);
                totalDebt += item.Debt;
            }
            // bước này là add thêm chính thằng tạo receipt vào chỗ detail, để nhìn cho cân bằng
            UserAvatarNameMoney userCreate = new UserAvatarNameMoney();
            userCreate.Avatar = creator.Avatar;
            userCreate.Name = creator.UserName;
            userCreate.Phone = await memberRepository.GetPhoneByUserId(creator.ID);
            userCreate.Role = await memberRepository.GetRole(receipt.EventID, creator.ID);
            userCreate.TotalAmount = receipt.ReceiptAmount - totalDebt;
            userCreate.TotalAmountFormat = format.MoneyFormat(userCreate.TotalAmount);
            // nhưng mà nếu tiền của nó mà bằng 0 thì cũng không hiện nữa
            if (receipt.ReceiptAmount - totalDebt > 0)
            {
                userDepts.Add(userCreate);
                userDepts.Reverse();
            }
            result.UserDepts = userDepts;
            result.ImgLink = await GetListImg("receipt", receiptId);
            result.ReceiptStatus = receipt.ReceiptStatus;
            return result;
        }

        private async Task<List<string>> GetListImg(string type, int receiptId)
        {
            return await myDB.ProofImages
                .Where(p => p.ImageType.Equals(type) && p.ModelId == receiptId)
                .Select(p => p.ImageLink)
                .ToListAsync();
        }

        // hiện ra những receipt đang chờ duyệt hoặc các receipt đã được duyệt(hoặc từ chôi)
        public async Task<List<ReceiptSentParam>> ReceiptsWaitingOrHandled(int userIdqeq,
            int eventId, bool isWaiting)
        {
            List<ReceiptSentParam> list = new List<ReceiptSentParam>();
            // lấy tất cả chứng từ trong event này
            List<Receipt> receipts = await myDB.Receipts.Include(r => r.User)
                .OrderByDescending(r => r.UpdatedAt)
                .Where(r => r.EventID == eventId)
                .ToListAsync();

            foreach (Receipt receipt in receipts)
            {
                ReceiptSentParam param = new ReceiptSentParam();
                param.ReceiptId = receipt.Id;
                param.Date = format.DateFormat(receipt.CreatedAt);
                param.ReceiptName = receipt.ReceiptName;
                param.ReceiptAmount = receipt.ReceiptAmount;
                param.ReceiptAmountFormat = format.MoneyFormat(receipt.ReceiptAmount);

                // kiểm tra nếu đang ở màn chứng từ chờ duyệt thì chỉ lấy status = 1
                if (isWaiting == true && receipt.ReceiptStatus != 1)
                    continue;
                // nếu đang ở màn đã xử lý thì không lấy status = 1
                if (isWaiting == false && receipt.ReceiptStatus == 1)
                    continue;
                param.ReceiptStatus = receipt.ReceiptStatus;
                param.ImageLinks = await myDB.ProofImages
                    .Where(p => p.ImageType.Equals("receipt") && p.ModelId == receipt.Id)
                    .Select(p => p.ImageLink).ToListAsync();

                // lấy ra cả tên của ông tạo ra receipt này
                param.User = new UserAvatarName
                {
                    Avatar = receipt.User.Avatar,
                    Name = receipt.User.UserName,
                    Phone = await memberRepository.GetPhoneByUserId(receipt.UserID),
                    Role = await memberRepository.GetRole(eventId, receipt.UserID)
                };

                list.Add(param);
            }
            return list;
        }

        // các chứng từ mà mình đã gửi, lấy cả 5 trạng thái
        public async Task<List<ReceiptSentParam>> ReceiptSent(int userId, int eventId)
        {
            List<ReceiptSentParam> list = new List<ReceiptSentParam>();
            List<Receipt> receipts = await myDB.Receipts.Include(r => r.User).Include(r => r.User.Account)
               .OrderByDescending(r => r.UpdatedAt)
               .Where(r => r.EventID == eventId && r.UserID == userId)
               .ToListAsync();
            foreach (Receipt receipt in receipts)
            {
                ReceiptSentParam param = new ReceiptSentParam();
                param.ReceiptId = receipt.Id;
                param.Date = receipt.CreatedAt.ToString();
                param.ReceiptName = receipt.ReceiptName;
                param.ReceiptAmount = receipt.ReceiptAmount;
                param.ReceiptAmountFormat = format.MoneyFormat(receipt.ReceiptAmount);
                param.ReceiptStatus = receipt.ReceiptStatus;
                param.ImageLinks = await myDB.ProofImages
                    .Where(p => p.ImageType.Equals("receipt") && p.ModelId == receipt.Id)
                    .Select(p => p.ImageLink).ToListAsync();
                // lấy ra cả tên của ông tạo ra receipt này
                param.User = new UserAvatarName
                {
                    Avatar = receipt.User.Avatar,
                    Name = receipt.User.UserName,
                    Phone = receipt.User.Account.PhoneNumber
                };
                list.Add(param);
            }
            return list;
        }

        private async Task<User> GetCashier(int eventId)
        {
            EventUser cashier = await myDB.EventUsers.Include(e => e.User)
                .FirstOrDefaultAsync(u => u.EventID == eventId && u.UserRole == 3);
            EventUser owner = await myDB.EventUsers.Include(e => e.User)
                .FirstOrDefaultAsync(u => u.EventID == eventId && u.UserRole == 1);
            EventUser inspector = await myDB.EventUsers.Include(e => e.User)
                .FirstOrDefaultAsync(u => u.EventID == eventId && u.UserRole == 2);
            if (cashier != null) return cashier.User;
            else if (owner != null) return owner.User;
            return inspector.User;
        }

        public async Task<bool> IsInspector(int eventId, int userId)
        {
            EventUser eu = await myDB.EventUsers
                .FirstOrDefaultAsync(ee => ee.EventID == eventId && ee.UserID == userId);
            if (eu.UserRole == 2) return true;
            else if (eu.UserRole == 1) return true;
            return false;
        }
        public async Task<bool> IsCashier(int eventId, int userId)
        {
            EventUser eu = await myDB.EventUsers
                .FirstOrDefaultAsync(ee => ee.EventID == eventId && ee.UserID == userId);
            if (eu.UserRole == 3) return true;
            else if (eu.UserRole == 1) return true;
            return false;
        }
        public async Task<bool> IsOwner(int eventId, int userId)
        {
            EventUser eu = await myDB.EventUsers
                .FirstOrDefaultAsync(ee => ee.EventID == eventId && ee.UserID == userId);
            return eu.UserRole == 1;
        }

        // duyệt chứng từ
        public async Task ReceiptApprove(ListIdStatus list, int userId)
        {
            foreach (int item in list.ListId)
            {
                Receipt receipt = await myDB.Receipts
                    .Include(r => r.UserDepts)
                    .Include(r => r.Event)
                    .FirstOrDefaultAsync(r => r.Id == item);
                receipt.ReceiptStatus = list.Status;
                receipt.UpdatedAt = DateTime.Now;
                // Add activity
                await activity.InspectorReceiptApproveActivity(list.Status, userId, receipt.ReceiptName
                    , receipt.Event.EventName);
                await activity.CreatorReceiptApproveActivity(list.Status, receipt.UserID
                    , receipt.ReceiptName, receipt.Event.EventName);
                receipt.UserDepts.ForEach(s => s.DeptStatus = list.Status);
                myDB.Receipts.Update(receipt);
                await myDB.SaveChangesAsync();
                // nếu tổng số tiền thằng tạo receipt nợ và số nó chờ nhận lại bằng nhau
                // thì sửa lại hết status của receipt nó tạo và nợ của nó thành đã trả hết
                //await CheckStatusChange(receipt.UserID, receipt.EventID);
            }
        }

        private async Task CheckStatusChange(int userID, int eventID)
        {
            double totalDebt = (await GetDebtMoney(eventID, userID)).Money.Amount;
            double totalReceive = (await GetReceiveMoney(eventID, userID)).Money.Amount;
            double diff = totalDebt - totalReceive;
            if (diff < 0) diff = diff * (-1);
            // nếu chênh lệch khoảng hơn 2k thì không tính nữa
            if (diff > 2000) return;
            // tất cả các chứng từ mình tạo sẽ chuyển trạng thái thành đã trả hết
            List<Receipt> receipts = await myDB.Receipts
                .Include(r => r.UserDepts)
                .Where(r => r.EventID == eventID && r.UserID == userID && r.ReceiptStatus == 2)
                .ToListAsync();
            foreach (var item in receipts)
            {
                item.ReceiptStatus = 0;
                item.UserDepts.ForEach(u => u.DeptStatus = 0);
                myDB.Receipts.Update(item);
                await myDB.SaveChangesAsync();
            }
            // tất cả các khoản nợ của mình cũng chuyển thành đã trả hết
            List<Receipt> allReceipts = await myDB.Receipts
                .Include(r => r.UserDepts)
                .Where(r => r.EventID == eventID && r.ReceiptStatus == 2).ToListAsync();
            foreach (Receipt item in allReceipts)
            {
                UserDept userDepts = item.UserDepts.FirstOrDefault(u => u.UserId == userID);
                if (userDepts != null)
                {
                    userDepts.DeptStatus = 0;
                    await myDB.SaveChangesAsync();
                    // đoạn này phải thêm 1 bước nữa là check cả hoá đơn cho thằng chủ nợ
                    // nghĩa là mình nợ nó 300k, h mình trả nó 300 thì phải xem nó có đang nợ ai đúng 300 không
                    // nếu có thì chuyển cái hoá đơn nợ 300k của nó thành đã trả hết
                    //await CheckOwnerDebt(userDepts);
                }
            }
            // kiểm tra trong event này, tất cả các user dept của cái nào mà trả hết rồi
            // thì chuyển receipt status thành 0
            await ChangeAllReceiptStatusInEvent(eventID);
        }

        // kiểm tra trong event này, tất cả các user dept của cái nào mà trả hết rồi
        // thì chuyển receipt status thành 0
        private async Task ChangeAllReceiptStatusInEvent(int eventId)
        {
            List<Receipt> receipts = await myDB.Receipts.Include(r => r.UserDepts)
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
                    await myDB.SaveChangesAsync();
                }
            }
        }
    }
}
