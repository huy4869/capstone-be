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
                .Where(ud => ud.ReceiptID == ReceiptID)
                .ToListAsync();
            return listUserDept;
        }
        public async Task<List<UserDept>> GetReceiptByUserIDAsync(int UserID)//NOTDONE
        {
            List<UserDept> listUserDept = await myDB.UserDepts//.Include(u => u.User)
                .Where(e => e.UserID == UserID)
                .ToListAsync();
            return listUserDept;
        }

        public async Task<int> AddUserDeptToReceiptAsync(UserDept addUserDept, int receiptID)//
        {
            UserDept storeUserDept = new UserDept();

            storeUserDept.ReceiptID = receiptID;
            storeUserDept.UserID = addUserDept.UserID;
            storeUserDept.DeptStatus = 2;
            storeUserDept.Debit = addUserDept.Debit;

            await myDB.UserDepts.AddAsync(storeUserDept);
            await myDB.SaveChangesAsync();

            return storeUserDept.DeptId;
        }


    }
}
