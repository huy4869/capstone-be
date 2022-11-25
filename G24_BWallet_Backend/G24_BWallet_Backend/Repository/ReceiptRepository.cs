using Amazon.S3.Transfer;
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

namespace G24_BWallet_Backend.Repository
{
    public class ReceiptRepository : IReceiptRepository
    {
        private readonly MyDBContext myDB;
        private readonly IConfiguration _configuration;

        public ReceiptRepository(MyDBContext myDB, IConfiguration _configuration)
        {
            this.myDB = myDB;
            this._configuration = _configuration;

        }

        public async Task<Receipt> AddReceiptAsync(ReceiptCreateParam addReceipt)//
        {
            Receipt storeReceipt = new Receipt();
            storeReceipt.EventID = addReceipt.EventID;
            storeReceipt.UserID = addReceipt.UserID;
            storeReceipt.ReceiptName = addReceipt.ReceiptName;
            storeReceipt.ReceiptAmount = addReceipt.ReceiptAmount;
            storeReceipt.ReceiptStatus = 2;

            DateTime VNDateTimeNow = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
            storeReceipt.CreatedAt = VNDateTimeNow;
            storeReceipt.UpdatedAt = VNDateTimeNow;

            await myDB.Receipts.AddAsync(storeReceipt);
            await myDB.SaveChangesAsync();

            return storeReceipt;
        }

        public async Task<ReceiptDetail> GetReceiptByIDAsync (int ReceiptID)
        {

            ReceiptDetail r = myDB.Receipts.Where(r => r.Id == ReceiptID).Include(r => r.User)
                .Select( r => new ReceiptDetail
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

        public async Task<EventReceiptsInfo> GetEventReceiptsInfoAsync(int EventID)
        {
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
