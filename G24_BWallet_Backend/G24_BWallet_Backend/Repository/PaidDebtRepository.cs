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
                .Where(r => r.EventID == eventId && r.ReceiptStatus == status).ToListAsync();
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
            return await Task.FromResult(userDepts);
        }

        public async Task<string> PaidDebtInEvent(PaidDebtParam p)
        {
            PaidDept paidDept = new PaidDept
            {
                UserId = p.UserId,
                EventId = p.EventId,
                PaidProof = p.PaidImage,
                TotalMoney = p.TotalMoney,
                Status = 1,
                UpdatedAt = System.DateTime.Now,
                CreatedAt = System.DateTime.Now
            };
            await context.PaidDepts.AddAsync(paidDept);
            await context.SaveChangesAsync();
            foreach (var item in p.ListEachPaidDebt)
            {
                PaidDebtList paid = new PaidDebtList {
                PaidId = paidDept.Id,
                DebtId = item.DebtId,
                PaidAmount = item.PaidAmount
                };
                await context.PaidDebtLists.AddAsync(paid);
                await context.SaveChangesAsync();
                await ChangeDebtLeft(item);
            }
            return "Đã cập nhật các khoản nợ và trạng thái";
        }

        private async Task ChangeDebtLeft(PaidDebtList item)
        {
            var userDebt = await context.UserDepts.FirstOrDefaultAsync(u=>u.Id == item.DebtId);
            userDebt.DebtLeft -= item.PaidAmount;
            if(userDebt.DebtLeft <= 0)// tra het no
            {
                userDebt.DeptStatus = 0;
            }
            await context.SaveChangesAsync();
        }
    }
}
