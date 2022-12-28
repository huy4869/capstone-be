using G24_BWallet_Backend.DBContexts;
using G24_BWallet_Backend.Models;
using G24_BWallet_Backend.Models.ObjectType;
using G24_BWallet_Backend.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twilio.TwiML.Fax;

namespace G24_BWallet_Backend.Repository
{
    public class ReportRepository : IReportRepository
    {
        private readonly MyDBContext context;
        private readonly ActivityRepository activity;
        private readonly Format format;

        public ReportRepository(MyDBContext myDB)
        {
            this.context = myDB;
            this.activity = new ActivityRepository(myDB);
            format = new Format();
        }
        public async Task<Report> GetReportByID(int reportID)
        {
            var report = await context.Reports.FindAsync(reportID);
            return report;
        }
        public async Task<List<ReportReturn>> GetListReport(int eventId)
        {
            var list = context.Reports.Where(re => re.EventId == eventId && re.ReportStatus == 0)
                .Include(re => re.User)
                .Include(re => re.User.Account)
                .Select(re => new ReportReturn
                {
                    ID = re.ID,
                    ReportReceiptID = re.ReceiptId,
                    ReportReceiptName = re.Receipt.ReceiptName,
                    ReportReason = re.ReportReason,
                    ReportStatus = re.ReportStatus,
                    CreatedAt = format.DateFormat(re.CreatedAt),
                    Reporter = new Member
                    {
                        UserId = re.User.ID,
                        UserName = re.User.UserName,
                        UserAvatar = re.User.Avatar,
                        UserPhone = re.User.Account.PhoneNumber
                    }
                })
                .ToListAsync();
            return await list;
        }

        public async Task<List<ReportReturn>> GetSolvedReports(int eventId)
        {
            var list = context.Reports.Where(re => re.EventId == eventId && re.ReportStatus != 0)
                .Include(re => re.User)
                .Include(re => re.User.Account)
                .Select(re => new ReportReturn
                {
                    ID = re.ID,
                    ReportReceiptID = re.ReceiptId,
                    ReportReceiptName = re.Receipt.ReceiptName,
                    ReportReason = re.ReportReason,
                    ReportStatus = re.ReportStatus,
                    CreatedAt = format.DateFormat(re.CreatedAt),
                    Reporter = new Member
                    {
                        UserId = re.User.ID,
                        UserName = re.User.UserName,
                        UserAvatar = re.User.Avatar,
                        UserPhone = re.User.Account.PhoneNumber
                    }
                })
                .ToListAsync();
            return await list;
        }

        // tạo report
        public async Task<string> createReport(int receiptID, int userID, string reason)
        {
            //check chứng từ đã có ai trả nợ chưa
            List<int> listUserDeptId = context.UserDepts.Where(ud => ud.ReceiptId == receiptID).Select(ud => ud.Id).ToList();
            PaidDebtList pdl = context.PaidDebtLists.Include(pdl => pdl.PaidDept).Where(pdl => listUserDeptId.Contains(pdl.DebtId) && pdl.PaidDept.Status == 2).FirstOrDefault();
            if (pdl != null)
            {
                return ("Không thể báo cáo chứng từ đang trong quá trình trả nợ!");
            }

            //kiểm tra chứng từ đã bị báo cáo chưa
            Report report = context.Reports.Where(re => re.ReceiptId == receiptID && re.ReportStatus != 2)
                .FirstOrDefault();
            if (report != null) return ("Chứng từ này đã bị báo cáo.");

            Receipt receipt = context.Receipts.Include(r => r.UserDepts).Include(r => r.Event)
                .Where(r => r.Id == receiptID).FirstOrDefault();
            if (receipt == null) return ("Chứng từ này không còn tồn tại!");
            else if (receipt.ReceiptStatus != 2) return ("Không thể báo cáo! Chứng từ này đã được thanh toán hết.");

            Report addReport = new Report();
            addReport.EventId = receipt.EventID;
            addReport.ReceiptId = receiptID;
            addReport.UserId = userID;
            addReport.ReportReason = reason;
            addReport.ReportStatus = 0;

            DateTime VNDateTimeNow = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
            addReport.CreatedAt = VNDateTimeNow;
            addReport.UpdatedAt = VNDateTimeNow;
            context.Reports.Add(addReport);

            //receipt and userDept change status
            receipt.ReceiptStatus = 4;
            //update userDept status bi tố cáo ngoại trừ userDept đã trả hết nợ
            receipt.UserDepts.ForEach(ud => ud.DeptStatus = (ud.DeptStatus != 0) ? 4 : 0);
            context.Receipts.Update(receipt);
            await activity.ReportActivity(1, 0, userID, receipt.ReceiptName, receipt.Event.EventName);
            await context.SaveChangesAsync();
            return "Đã báo cáo chứng từ thành công";
        }

        // xủ lý chấp thuận hay từ chối báo cáo
        public async Task<string> responeReport(int reportId, int status, int userId)
        {
            Report report = context.Reports.Where(re => re.ID == reportId).FirstOrDefault();
            if (report == null) return ("Báo cáo không còn tồn tại!");
            else if (report.ReportStatus != 0) return ("Báo cáo này đã được xử lý.");

            Receipt receipt = context.Receipts.Include(r => r.UserDepts)
                .Include(r => r.Event).Where(r => r.Id == report.ReceiptId).FirstOrDefault();
            switch (status)
            {
                case 1://accept
                    receipt.ReceiptStatus = 5;
                    receipt.UserDepts.ForEach(ud => ud.DeptStatus = (ud.DeptStatus != 0) ? 5 : 0);
                    context.Receipts.Update(receipt);


                    List<int> userDeptsIDList = receipt.UserDepts.Select(ud => ud.Id).ToList();

                    //tìm những paiddept liên quan mà đang chờ duyệt xóa trừ đi những khoản userDept liên quan
                    var listPaidDeptListsID = await context.PaidDebtLists.Include(pdl => pdl.PaidDept)
                        .Where(pdl => userDeptsIDList.Contains(pdl.DebtId) && pdl.PaidDept.Status == 1)
                        .Select(ud => new { ud.Id, ud.PaidId })
                        .ToListAsync();

                    PaidDebtList pdlHolder;
                    PaidDept pdHolder;
                    double removeAmount = 0;
                    foreach (var item in listPaidDeptListsID)
                    {
                        //bỏ những khoảng trả bị report
                        pdlHolder = context.PaidDebtLists.Where(pdl => pdl.Id == item.Id).First();
                        removeAmount = pdlHolder.PaidAmount;
                        pdlHolder.PaidAmount = 0;
                        context.PaidDebtLists.Update(pdlHolder);
                        await context.SaveChangesAsync();

                        //trừ những khoảng trả bị report
                        pdHolder = context.PaidDepts.Where(pd => pd.Id == item.PaidId).First();
                        pdHolder.TotalMoney -= removeAmount;
                        context.PaidDepts.Update(pdHolder);
                        await context.SaveChangesAsync();
                    }
                    await activity.ReportActivity(2, 1, userId, receipt.ReceiptName, receipt.Event.EventName);
                    await activity.ReportActivity(3, 1, report.UserId, receipt.ReceiptName, receipt.Event.EventName);
                    break;

                case 2://deny
                    receipt.ReceiptStatus = 2;
                    receipt.UserDepts.ForEach(ud => ud.DeptStatus = (ud.DeptStatus != 0) ? 2 : 0);
                    context.Receipts.Update(receipt);
                    await activity.ReportActivity(2, 0, userId, receipt.ReceiptName, receipt.Event.EventName);
                    await activity.ReportActivity(3, 0, report.UserId, receipt.ReceiptName, receipt.Event.EventName);
                    break;

                default: throw new Exception("Báo cáo xử lý bị lỗi!");
            }
            report.ReportStatus = status;
            context.Reports.Update(report);

            await context.SaveChangesAsync();
            return "Đã xử lý báo cáo.";
        }
    }
}
