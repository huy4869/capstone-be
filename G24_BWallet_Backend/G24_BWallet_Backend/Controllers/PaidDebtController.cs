using G24_BWallet_Backend.Models.ObjectType;
using G24_BWallet_Backend.Models;
using G24_BWallet_Backend.Repository.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Collections;
using Twilio.TwiML.Fax;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System;

namespace G24_BWallet_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PaidDebtController : ControllerBase
    {
        private readonly IPaidDebtRepository paidDeptRepo;
        private readonly IImageRepository imageRepo;

        public PaidDebtController(IPaidDebtRepository paidDeptRepo, IImageRepository imageRepo)
        {
            this.paidDeptRepo = paidDeptRepo;
            this.imageRepo = imageRepo;
        }
        protected int GetUserId()
        {
            return int.Parse(this.User.Claims.First(i => i.Type == "UserId").Value);
        }
        [HttpPost("listDebt")]
        public async Task<Respond<List<UserDebtReturn>>> GetListUserDebt(EventUserID e)
        {
            e.UserId = GetUserId();
            int status = 2;
            List<Receipt> receipt = await paidDeptRepo.GetReceipts(e.EventId, status);
            List<UserDebtReturn> userDepts = await paidDeptRepo.GetUserDepts(receipt, e.UserId);
            return new Respond<List<UserDebtReturn>>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Danh sách các hoá đơn mình còn nợ trong event này",
                //Data = new ArrayList { new JWT(await jwt),await user}
                Data = userDepts
            };

        }

        [HttpPost("paidDebt")]
        public async Task<Respond<PaidDept>> CreatePaidDebt(PaidDebtParam paidParam)
        {
            paidParam.UserId = GetUserId();
            if (!paidParam.IMGLinks.Any())
            {
                return new Respond<PaidDept>()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Error = "những khoảng trả không có ảnh chứng minh",
                    Message = "",
                    Data = null
                };
            }

            var paid = await paidDeptRepo.PaidDebtInEvent(paidParam);

            await imageRepo.AddIMGLinksDB("paidDept", paid.Id, paidParam.IMGLinks);

            return new Respond<PaidDept>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "ghi chú khoảng trả, chờ xác thực",
                Data = null
            };

        }

        [HttpGet("paid-code")]
        public async Task<Respond<string>> GetCodePaidDebt()
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var code = new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            return new Respond<string>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Mã chứng từ thanh toán",
                Data = "BW_" + code
            };

        }

        [HttpGet("paid-sent/eventId={eventId}")]
        public async Task<Respond<List<DebtPaymentPending>>> PaidDebtRequestSent(int eventId)
        {
            bool isWaiting = false;
            List<DebtPaymentPending> list = await paidDeptRepo
                .PaidDebtRequestSent(GetUserId(), eventId, isWaiting);
            return new Respond<List<DebtPaymentPending>>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Yêu cầu trả tiền đã gửi",
                Data = list
            };
        }

        [HttpGet("paidSent-waiting/eventId={eventId}")]
        public async Task<Respond<List<DebtPaymentPending>>> PaidSentWaiting(int eventId)
        {
            bool isWaiting = true;
            List<DebtPaymentPending> list = await paidDeptRepo.PaidDebtRequestSent(GetUserId(), eventId, isWaiting);
            return new Respond<List<DebtPaymentPending>>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Yêu cầu trả tiền đang chờ duyệt",
                Data = list
            };
        }

        [HttpPost("paid-approve")]
        public async Task<Respond<string>> PaidDebtApprove(ListIdStatus list)
        {
            await paidDeptRepo.PaidDebtApprove(list,GetUserId());
            return new Respond<string>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Đã phê duyệt hoặc từ chối các yêu cầu trả tiền(có thể xem lịch sử)",
                Data = null
            };

        }

        //[HttpGet("paid-detail/PaidId={paidid}")]
        //public async Task<Respond<string>> PaidDebtDetail(int paidid)
        //{
        //    await paidDeptRepo.PaidDebtApprove(list, GetUserId());
        //    return new Respond<string>()
        //    {
        //        StatusCode = HttpStatusCode.Accepted,
        //        Error = "",
        //        Message = "Chi tiết các yêu cầu trả tiền khi click vào",
        //        Data = null
        //    };

        //}
    }
}
