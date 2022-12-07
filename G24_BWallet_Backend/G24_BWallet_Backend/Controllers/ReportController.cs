using G24_BWallet_Backend.Models;
using G24_BWallet_Backend.Models.ObjectType;
using G24_BWallet_Backend.Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace G24_BWallet_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]

    public class ReportController : ControllerBase
    {
        private readonly IReportRepository reportRepo;
        private readonly IMemberRepository memberRepo;

        public ReportController(IReportRepository InitReportRepo, IMemberRepository memberRepo)
        {
            this.reportRepo = InitReportRepo;
            this.memberRepo = memberRepo;
        }
        protected int GetUserId()
        {
            return int.Parse(this.User.Claims.First(i => i.Type == "UserId").Value);
        }

        [HttpGet]
        public async Task<Respond<List<ReportReturn>>> GetEventReports([FromQuery] int eventid)
        {
            //List<ReportReturn>
            int userID = GetUserId();
            var listReport = reportRepo.GetListReport(eventid);

            return new Respond<List<ReportReturn>>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "danh sách báo cáo chờ duyệt",
                Data = await listReport
            };
        }

        [HttpGet("history")]
        public async Task<Respond<List<ReportReturn>>> GetHistoryReports([FromQuery] int eventid)
        {
            int userID = GetUserId();
            var listReport = reportRepo.GetSolvedReports(eventid);

            return new Respond<List<ReportReturn>>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "danh sách báo cáo đã duyệt",
                Data = await listReport
            };
        }

        [HttpPost]
        public async Task<Respond<Report>> createReport([FromBody] Report reportInfo)
        {
            int userID = GetUserId();
            
            if (reportInfo.ReportReason.IsNullOrEmpty() || reportInfo.ReceiptId == 0)
            {
                return new Respond<Report>()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Error = "",
                    Message = "báo cáo thiếu thông tin",
                    Data = null
                };
            }
            var result = await reportRepo.createReport(reportInfo.ReceiptId, userID, reportInfo.ReportReason);
            
            return new Respond<Report>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "đã báo cáo chứng từ thành công",
                Data = null
            };
        }

        [HttpPost("respone-report")]
        public async Task<Respond<Report>> RespondReport([FromBody] Report reportInfo)
        {
            int userID = GetUserId();
            int eventID = reportRepo.GetReportByID(reportInfo.ID).Result.EventId;

           /* if (!await memberRepo.IsOwner(eventID, userID))
                return new Respond<Report>()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Error = "",
                    Message = "bạn không có quyền xử lý báo cáo",
                    Data = null
                };*/
            var result = await reportRepo.responeReport(reportInfo.ID, reportInfo.ReportStatus);

            return new Respond<Report>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "đã xử lý báo cáo",
                Data = null
            };
        }
    }
}
