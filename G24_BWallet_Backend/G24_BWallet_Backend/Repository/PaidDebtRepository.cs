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

        public PaidDebtRepository(MyDBContext myDB)
        {
            this.context = myDB;
        }
        public async Task<List<Receipt>> GetReceipts(int eventId, int status)
        {
            var list = context.Receipts.Include(r => r.UserDepts).Include(r => r.User)
                .Where(r => r.EventID == eventId && r.ReceiptStatus == status)
                .OrderByDescending(r => r.Id)
                .ToListAsync();
            return await list;
        }

        public async Task<List<UserDebtReturn>> GetUserDepts(List<Receipt> receipt, int userId)
        {
            List<UserDebtReturn> userDepts = new List<UserDebtReturn>();
            foreach (var item in receipt)
            {
                UserDept ud = item.UserDepts.Where(ud => ud.UserId == userId).FirstOrDefault();
                if (ud != null)
                {
                    UserDebtReturn udr = new UserDebtReturn();
                    udr.UserDeptId = ud.Id;
                    udr.ReceiptName = item.ReceiptName;
                    udr.Date = item.CreatedAt + "";
                    udr.OwnerName = item.User.UserName;
                    udr.DebtLeft = ud.DebtLeft;
                    userDepts.Add(udr);
                }

            }
            userDepts.Reverse();
            return await Task.FromResult(userDepts);
        }

        public async Task<PaidDept> PaidDebtInEvent(PaidDebtParam p)//create paid dept
        {
            DateTime VNDateTimeNow = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
            PaidDept paidDept = new PaidDept
            {
                UserId = p.UserId,
                EventId = p.EventId,
                TotalMoney = p.TotalMoney,
                Status = 1,
                UpdatedAt = VNDateTimeNow,
                CreatedAt = VNDateTimeNow
            };
            try
            {
                await context.PaidDepts.AddAsync(paidDept);

                foreach (var item in p.ListEachPaidDebt)
                {
                    PaidDebtList paid = new PaidDebtList
                    {
                        PaidId = paidDept.Id,
                        DebtId = item.userDeptId,
                        PaidAmount = item.debtLeft
                    };
                    await context.PaidDebtLists.AddAsync(paid);
                    await ChangeDebtLeft(item);
                }
                await context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw new Exception("PaidDept:Lỗi ghi tiền trả");
            }
            return paidDept;
        }

        private async Task ChangeDebtLeft(RenamePaidDebtList item)
        {
            var paiddlist = new PaidDebtList
            {
                DebtId = item.userDeptId,
                PaidAmount = item.debtLeft
            };
            var userDebt = await context.UserDepts.FirstOrDefaultAsync(u => u.Id == paiddlist.DebtId);
            userDebt.DebtLeft -= paiddlist.PaidAmount;
            if (userDebt.DebtLeft <= 0)// tra het no
            {
                userDebt.DeptStatus = 0;
            }
            await context.SaveChangesAsync();
        }
    }
}
