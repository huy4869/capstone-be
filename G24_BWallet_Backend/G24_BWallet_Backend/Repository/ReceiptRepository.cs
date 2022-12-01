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

namespace G24_BWallet_Backend.Repository
{
    public class ReceiptRepository : IReceiptRepository
    {
        private readonly MyDBContext myDB;
        private readonly IConfiguration _configuration;
        private readonly Format format;

        public ReceiptRepository(MyDBContext myDB, IConfiguration _configuration )
        {
            this.myDB = myDB;
            this._configuration = _configuration;
            this.format = new Format();

        }

        public async Task<Receipt> AddReceiptAsync(ReceiptCreateParam addReceipt)//
        {
            Receipt storeReceipt = new Receipt();
            storeReceipt.EventID = addReceipt.EventID;
            storeReceipt.UserID = addReceipt.UserID;
            storeReceipt.ReceiptName = addReceipt.ReceiptName;
            storeReceipt.ReceiptAmount = addReceipt.ReceiptAmount;
            storeReceipt.ReceiptStatus = 1;

            DateTime VNDateTimeNow = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
            storeReceipt.CreatedAt = VNDateTimeNow;
            storeReceipt.UpdatedAt = VNDateTimeNow;

            await myDB.Receipts.AddAsync(storeReceipt);
            await myDB.SaveChangesAsync();

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
                    DebtLeft = ud.DebtLeft
                }).ToListAsync();

            r.IMGLinks = listIMG;
            r.ListUserDepts = await listUDept;
            return r;
        }

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
                .Where(r => r.ReceiptStatus == 2)
                .Select(r => new ReceiptMainInfo
                {
                    Id = r.Id,
                    ReceiptName = r.ReceiptName,
                    ReceiptAmount = r.ReceiptAmount,
                    ReceiptAmountFormat = format.MoneyFormat(r.ReceiptAmount),
                    CreatedAt = r.CreatedAt
                })
                .ToList();
            //eventInfo.TotalReceiptsAmount = listReceipt.Sum(r => r.ReceiptAmount);
            NumberMoney Debt = await GetDebtMoney(EventID, userID);
            NumberMoney Receive = await GetReceiveMoney(EventID, userID);
            eventInfo.UserInvolveAmount = await GetTotalMoney(Debt, Receive);

            eventInfo.listReceipt = listReceipt;

            return eventInfo;
        }
        private async Task<NumberMoney> GetDebtMoney(int eventId, int userID)
        {
            NumberMoney number = new NumberMoney();
            MoneyColor moneyColor = new MoneyColor();
            double mon = 0;
            int total = 0;
            List<int> userIdList = new List<int>();
            // lấy hết các receipt đang trả trong event này
            List<Receipt> receiptList = await myDB.Receipts
                .Where(r => r.EventID == eventId && r.ReceiptStatus == 2).ToListAsync();
            foreach (Receipt receipt in receiptList)
            {
                UserDept userDept = await myDB.UserDepts
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

        private async Task<NumberMoney> GetReceiveMoney(int eventId, int userID)
        {
            NumberMoney number = new NumberMoney();
            MoneyColor moneyColor = new MoneyColor();
            double mon = 0;
            int total = 0;
            List<int> userIdList = new List<int>();
            // lấy hết các receipt mình tạo trong event này mà vẫn đang trả
            List<Receipt> receiptList = await myDB.Receipts
                .Where(r => r.EventID == eventId && r.ReceiptStatus == 2
                && r.UserID == userID).ToListAsync();
            foreach (Receipt receipt in receiptList)
            {
                List<UserDept> userDepts = await myDB.UserDepts
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
        public async Task<MoneyColor> GetTotalMoney(NumberMoney debt, NumberMoney receive)
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


        public async Task<ReceiptUserDeptName> GetReceiptDetail(int receiptId)
        {
            ReceiptUserDeptName result = new ReceiptUserDeptName();
            Receipt receipt = await myDB.Receipts.Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == receiptId);
            result.ReceiptName = receipt.ReceiptName;
            result.Date = receipt.CreatedAt.ToString();
            User cashier = await GetCashier(receipt.EventID);
            result.User = new UserAvatarNameMoney
            {
                Avatar = cashier.Avatar,
                Name = cashier.UserName,
                TotalAmount = receipt.ReceiptAmount
            };
            List<UserAvatarNameMoney> userDepts = new List<UserAvatarNameMoney>();
            List<UserDept> depts = await myDB.UserDepts.Include(r => r.User)
                .Where(r => r.ReceiptId == receiptId).ToListAsync();
            foreach (UserDept item in depts)
            {
                UserAvatarNameMoney user = new UserAvatarNameMoney();
                user.Avatar = item.User.Avatar;
                user.Name = item.User.UserName;
                user.TotalAmount = item.Debt;
                userDepts.Add(user);
            }
            result.UserDepts = userDepts;
            return result;
        }

        public async Task<List<ReceiptSentParam>> ReceiptsSent(int userId, int eventId, bool isWaiting)
        {
            List<ReceiptSentParam> list = new List<ReceiptSentParam>();
            List<Receipt> receipts = await myDB.Receipts.Include(r => r.User)
                .Where(r => r.EventID == eventId && r.UserID == userId).ToListAsync();

            // nếu mình là inspector hoặc owner thì sẽ lấy tất cả chứng từ trong event này
            if (await IsInspector(eventId, userId) || await IsOwner(eventId,userId))
                receipts = await myDB.Receipts.Include(r => r.User)
                .Where(r => r.EventID == eventId).ToListAsync();

            foreach (Receipt receipt in receipts)
            {
                ReceiptSentParam param = new ReceiptSentParam();
                param.ReceiptId = receipt.Id;
                param.Date = receipt.CreatedAt.ToString();
                param.ReceiptName = receipt.ReceiptName;
                param.ReceiptAmount = receipt.ReceiptAmount;

                // kiểm tra nếu đang ở màn chứng từ chờ duyệt thì chỉ lấy status = 1
                if (isWaiting == true && receipt.ReceiptStatus != 1)
                    continue;
                param.ReceiptStatus = receipt.ReceiptStatus;
                param.ImageLinks = await myDB.ProofImages
                    .Where(p => p.ImageType.Equals("receipt") && p.ModelId == receipt.Id)
                    .Select(p => p.ImageLink).ToListAsync();

                // nếu mình là inspector thì sẽ lấy ra cả tên của ông tạo ra receipt này
                if (await IsInspector(eventId, userId))
                    param.User = new UserAvatarName
                    {
                        Avatar = receipt.User.Avatar,
                        Name = receipt.User.UserName
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
        public async Task<bool> IsOwner(int eventId, int userId)
        {
            EventUser eu = await myDB.EventUsers
                .FirstOrDefaultAsync(ee => ee.EventID == eventId && ee.UserID == userId);
            return eu.UserRole == 1;
        }
        public async Task ReceiptApprove(ListIdStatus list)
        {
            foreach (int item in list.ListId)
            {
                Receipt receipt = await myDB.Receipts.Include(r => r.UserDepts).FirstOrDefaultAsync(r => r.Id == item);
                receipt.ReceiptStatus = list.Status;
                receipt.UserDepts.ForEach(s => s.DeptStatus = list.Status);
                myDB.Receipts.Update(receipt);
                await myDB.SaveChangesAsync();
            }
        }
    }
}
