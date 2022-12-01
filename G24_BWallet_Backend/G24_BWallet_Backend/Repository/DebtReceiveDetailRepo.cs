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
    public class DebtReceiveDetailRepo : IDebtReceiveDetailRepo
    {
        private readonly MyDBContext context;
        private readonly Format format;

        public DebtReceiveDetailRepo(MyDBContext myDB)
        {
            this.context = myDB;
            format = new Format();
        }

        // tiền mình nợ trong event này
        private async Task<NumberMoney> GetDebtMoney(int eventId, int userID)
        {
            NumberMoney number = new NumberMoney();
            MoneyColor moneyColor = new MoneyColor();
            double mon = 0;
            int total = 0;
            List<int> userIdList = new List<int>();
            // lấy hết các receipt đang trả trong event này
            List<Receipt> receiptList = await context.Receipts
                .Where(r => r.EventID == eventId && r.ReceiptStatus == 2).ToListAsync();
            foreach (Receipt receipt in receiptList)
            {
                UserDept userDept = await context.UserDepts
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

        // tiền người ta nợ mình trong event này
        private async Task<NumberMoney> GetReceiveMoney(int eventId, int userID)
        {
            NumberMoney number = new NumberMoney();
            MoneyColor moneyColor = new MoneyColor();
            double mon = 0;
            int total = 0;
            List<int> userIdList = new List<int>();
            // lấy hết các receipt mình tạo trong event này mà vẫn đang trả
            List<Receipt> receiptList = await context.Receipts
                .Where(r => r.EventID == eventId && r.ReceiptStatus == 2
                && r.UserID == userID).ToListAsync();
            foreach (Receipt receipt in receiptList)
            {
                List<UserDept> userDepts = await context.UserDepts
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

        public async Task<TotalMoneyUser> GetAllDebtInEvent(int userId, int eventId)
        {
            TotalMoneyUser total = new TotalMoneyUser();
            List<IdAvatarNamePhoneMoney> list = new List<IdAvatarNamePhoneMoney>();
            total.Amount = (await GetDebtMoney(eventId, userId)).Money.AmountFormat;
            // lấy hết các receipt đang trả trong event này
            List<Receipt> receiptList = await context.Receipts
                .Include(r => r.User)
                .Where(r => r.EventID == eventId && r.ReceiptStatus == 2).ToListAsync();
            foreach (Receipt receipt in receiptList)
            {
                UserDept userDept = await context.UserDepts
                    .FirstOrDefaultAsync(u => u.UserId == userId && u.ReceiptId == receipt.Id
                    && u.DeptStatus == 2 && u.DebtLeft > 0);
                if (userDept != null)
                {
                    // lấy ra người tạo receipt này(nghĩa là mình đang nợ thằng tạo này)
                    User user = receipt.User;
                    IdAvatarNamePhoneMoney i = new IdAvatarNamePhoneMoney();
                    i.ReceiptId = receipt.Id;
                    i.Avatar = user.Avatar;
                    i.Name = user.UserName;
                    Account a = (await context.Users.Include(u => u.Account)
                        .FirstOrDefaultAsync(u => u.AccountID == user.AccountID)).Account;
                    i.Phone = a.PhoneNumber;
                    i.Money = format.MoneyFormat(userDept.DebtLeft);
                    list.Add(i);
                }
            }
            total.List = list;
            return total;
        }

        public async Task<TotalMoneyUser> GetAllReceiveInEvent(int userId, int eventId)
        {
            TotalMoneyUser total = new TotalMoneyUser();
            List<IdAvatarNamePhoneMoney> list = new List<IdAvatarNamePhoneMoney>();
            total.Amount = (await GetReceiveMoney(eventId, userId)).Money.AmountFormat;
            // lấy hết các receipt của mình tạo mà người ta vân chưa trả đủ
            List<Receipt> receiptList = await context.Receipts
                .Include(r => r.User)
                .Where(r => r.EventID == eventId && r.ReceiptStatus == 2
                && r.UserID == userId).OrderByDescending(r => r.Id).ToListAsync();
            foreach (Receipt receipt in receiptList)
            {
                List<UserDept> userDepts = await context.UserDepts
                    .Include(r => r.User)
                    .Where(r => r.ReceiptId == receipt.Id && r.DeptStatus == 2
                    && r.DebtLeft > 0).OrderByDescending(r => r.Id).ToListAsync();
                foreach (UserDept userDept in userDepts)
                {
                    if (userDept != null)
                    {
                        IdAvatarNamePhoneMoney i = new IdAvatarNamePhoneMoney();
                        i.ReceiptId = receipt.Id;
                        i.Avatar = userDept.User.Avatar;
                        i.Name = userDept.User.UserName;
                        User u = userDept.User;
                        Account a = (await context.Users.Include(u => u.Account)
                        .FirstOrDefaultAsync(u => u.ID == userDept.UserId)).Account;
                        i.Phone = a.PhoneNumber;
                        i.Money = format.MoneyFormat(userDept.DebtLeft);
                        list.Add(i);
                    }
                }
            }
            total.List = list;
            return total;
        }

        public async Task<ReceiptUserDeptName> ClickIButton(int receiptId, int userId)
        {
            ReceiptUserDeptName result = new ReceiptUserDeptName();
            Receipt receipt = await context.Receipts.Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == receiptId);
            result.ReceiptName = receipt.ReceiptName;
            result.Date = receipt.CreatedAt.ToString();
            result.User = new UserAvatarNameMoney
            {
                Avatar = receipt.User.Avatar,
                Name = receipt.User.UserName,
                TotalAmount = receipt.ReceiptAmount
            };
            List<UserAvatarNameMoney> userDepts = new List<UserAvatarNameMoney>();
            List<UserDept> depts = await context.UserDepts.Include(r => r.User)
                .Where(r => r.ReceiptId == receiptId).ToListAsync();
            depts = await ChangePosition(depts, userId);
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

        private async Task<List<UserDept>> ChangePosition(List<UserDept> depts, int userId)
        {
            int index = -1;
            UserDept temp = null;
            for (int i = 0; i < depts.Count; i++)
            {
                if (depts[i].UserId == userId)
                {
                    index = i;
                    break;
                }
            }
            if (index != -1) //có mình trong danh sách user dept thì chuyển lên đầu
            {
                temp = depts[index];
                depts.RemoveAt(index);
                depts.Insert(0, temp);
            }
            return await Task.FromResult(depts);
        }
    }
}
