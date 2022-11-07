using G24_BWallet_Backend.DBContexts;
using G24_BWallet_Backend.Models;
using G24_BWallet_Backend.Models.ObjectType;
using G24_BWallet_Backend.Repository.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace G24_BWallet_Backend.Repository
{
    public class ReceiptRepository : IReceiptRepository
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
            var r = myDB.Receipts.Where(r => r.Id == ReceiptID).ToListAsync();
            return await r;
        }

        public async Task<EventReceiptsInfo> GetEventReceiptsInfoAsync(int EventID)
        {
            /*   Warning Dont attempt // if you do try, try get event that have no receipt
            EventReceiptsInfo eventReceiptInfo = myDB.Events
                .SelectMany(r => r.Receipts, (e, r) => new { e, r })
                .Where(er => er.e.ID == EventID)
                .Where(er => er.r.ReceiptStatus == 2)
                .GroupBy(er => new
                {
                    er.e.ID,//eventID
                    er.e.EventName//EventName
                })
                .Select(er => new EventReceiptsInfo
                {
                    Id = er.Key.ID,//eventID
                    EventName = er.Key.EventName,
                    TotalReceipt = er.Sum(x => x.r.ReceiptAmount)//x là cái new ở chỗ selectMany
                }).FirstOrDefault();
            */
            EventReceiptsInfo eventInfo = myDB.Events
                .Where(e => e.ID == EventID)
                .Select(e => new EventReceiptsInfo { 
                    Id = e.ID,
                    EventName = e.EventName,
                    TotalReceiptsAmount = 0
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
                    CreatedAt = r.CreatedAt
                })
                .ToList();
            foreach (ReceiptMainInfo r in listReceipt)
            {
                eventInfo.TotalReceiptsAmount += r.ReceiptAmount;
            }
            eventInfo.listReceipt = listReceipt;

            return eventInfo;
        }
    }
}
