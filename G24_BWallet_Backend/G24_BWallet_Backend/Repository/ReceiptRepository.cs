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

        public async Task<Receipt> AddReceiptAsync(Receipt addReceipt, IFormFile imgFile)//
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

            //lưu ảnh với để ID của receipt đầu ảnh để  tránh ghi đè file trên s3 
            if (imgFile == null || 
                (!string.Equals(imgFile.ContentType, "image/jpg", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(imgFile.ContentType, "image/jpeg", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(imgFile.ContentType, "image/gif", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(imgFile.ContentType, "image/png", StringComparison.OrdinalIgnoreCase)))
            {
                return storeReceipt;
            }

            string AWSS3AccessKeyId = _configuration["AWSS3:AccessKeyId"];
            string AWSS3SecretAccessKey = _configuration["AWSS3:SecretAccessKey"];
            string fileName = storeReceipt.Id + imgFile.FileName;

            using (var client = new AmazonS3Client(AWSS3AccessKeyId, AWSS3SecretAccessKey, RegionEndpoint.APSoutheast1))
            {
                using (var newMemoryStream = new MemoryStream())
                {
                    imgFile.CopyTo(newMemoryStream);
                    var uploadRequest = new TransferUtilityUploadRequest
                    {
                        InputStream = newMemoryStream,
                        Key = fileName,
                        BucketName = "bwallets3bucket/receipts"
                    };
                    var fileTransferUtility = new TransferUtility(client);
                    await fileTransferUtility.UploadAsync(uploadRequest);
                }
            }

            storeReceipt.ReceiptPicture = _configuration["AWSS3:ReceiptsImgLink"] + fileName.Replace("+", "%2B").Replace(' ', '+');//aws link file nó thay dấu cách bằng dấu + và + thì thành %2B
            myDB.Receipts.Update(storeReceipt);
            await myDB.SaveChangesAsync();

            return storeReceipt;
        }

        public async Task<Receipt> GetReceiptByIDAsync (int ReceiptID)
        {
            var r = myDB.Receipts.Where(r => r.Id == ReceiptID).FirstAsync();//.Include(r => r.UserDepts).FirstOrDefault();
            //var listUD = myDB.UserDepts.Where(ud => ud.ReceiptId == ReceiptID).ToListAsync();
            //r.UserDepts = await listUD;
            return await r;
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
