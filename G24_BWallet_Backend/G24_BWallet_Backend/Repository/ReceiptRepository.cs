using G24_BWallet_Backend.DBContexts;
using G24_BWallet_Backend.Models;
using G24_BWallet_Backend.Repository.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace G24_BWallet_Backend.Repository
{
    public class ReceiptRepository :IReceiptRepository
    {
        private readonly MyDBContext myDB;

        public ReceiptRepository(MyDBContext myDB)
        {
            this.myDB = myDB;
        }

        public async Task<Receipt> AddReceiptAsync(Receipt addReceipt)//
        {
            Receipt storeReceipt = new Receipt();
            storeReceipt.EventID = addReceipt.EventID;
            storeReceipt.UserID = addReceipt.UserID;
            storeReceipt.ReceiptName = addReceipt.ReceiptName;
            storeReceipt.ReceiptAmount = addReceipt.ReceiptAmount;
            storeReceipt.ReceiptStatus = 2;
            storeReceipt.CreatedAt = DateTime.Now;
            storeReceipt.UpdatedAt = DateTime.Now;

            await myDB.Receipts.AddAsync(storeReceipt);
            await myDB.SaveChangesAsync();

            return storeReceipt;
        }

        public async Task<List<Receipt>> GetReceiptByIDAsync (int ReceiptID)
        {
            var r = myDB.Receipts.Where(r => r.ReceiptID == ReceiptID).ToListAsync();
            return await r;
        }

        public async Task<List<Receipt>> GetReceiptByEventIDAsync(int EventID)
        {
            List<Receipt> receiptList = await myDB.Receipts
                .Where(r => r.EventID == EventID)
                .ToListAsync();
            return receiptList;
        }
    }
}
