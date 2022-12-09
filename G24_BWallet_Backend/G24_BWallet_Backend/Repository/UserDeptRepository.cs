using G24_BWallet_Backend.DBContexts;
using G24_BWallet_Backend.Models;
using G24_BWallet_Backend.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace G24_BWallet_Backend.Repository
{
    public class UserDeptRepository : IUserDeptRepository
    {
        private readonly MyDBContext myDB;

        public UserDeptRepository(MyDBContext myDB)
        {
            this.myDB = myDB;
        }
        public async Task<List<UserDept>> GeUserDeptByReceiptIDAsync(int ReceiptID)//NOTDONE
        {
            List<UserDept> listUserDept = await myDB.UserDepts//.Include(u => u.User)
                .Where(ud => ud.ReceiptId == ReceiptID)
                .ToListAsync();
            return listUserDept;
        }
        public async Task<List<UserDept>> GetReceiptByUserIDAsync(int UserID)//NOTDONE
        {
            List<UserDept> listUserDept = await myDB.UserDepts//.Include(u => u.User)
                .Where(e => e.UserId == UserID)
                .ToListAsync();
            return listUserDept;
        }

        public async Task<int> AddUserDeptToReceiptAsync(UserDept addUserDept, int receiptID, int userRole)//
        {
            UserDept storeUserDept = new UserDept();

            storeUserDept.ReceiptId = receiptID;
            storeUserDept.UserId = addUserDept.UserId;

            if (addUserDept.Debt == 0)
                storeUserDept.DeptStatus = 0;
            else if (userRole == 1 || userRole == 2)
                storeUserDept.DeptStatus = 2;
            else
                storeUserDept.DeptStatus = 1;

            storeUserDept.Debt = addUserDept.Debt;
            storeUserDept.DebtLeft = addUserDept.Debt;

            await myDB.UserDepts.AddAsync(storeUserDept);
            await myDB.SaveChangesAsync();

            return storeUserDept.Id;
        }

    }
}
