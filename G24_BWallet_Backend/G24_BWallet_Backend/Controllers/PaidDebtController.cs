﻿using G24_BWallet_Backend.Models.ObjectType;
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
        private readonly IMemberRepository memberRepo;

        public PaidDebtController(IPaidDebtRepository paidDeptRepo, IImageRepository imageRepo,
            IMemberRepository memberRepository)
        {
            this.paidDeptRepo = paidDeptRepo;
            this.imageRepo = imageRepo;
            this.memberRepo = memberRepository;
        }
        protected int GetUserId()
        {
            return int.Parse(this.User.Claims.First(i => i.Type == "UserId").Value);
        }

        //Danh sách các hoá đơn mình còn nợ trong event này
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
                Message = "Danh sách các hoá đơn mình còn nợ trong sự kiện này",
                //Data = new ArrayList { new JWT(await jwt),await user}
                Data = userDepts
            };

        }

        // tạo 1 yêu cầu trả tiền
        [HttpPost("paidDebt")]
        public async Task<Respond<PaidDept>> CreatePaidDebt(PaidDebtParam paidParam)
        {
            try
            {
                paidParam.UserId = GetUserId();
                /*if (!paidParam.IMGLinks.Any())
                {
                    return new Respond<PaidDept>()
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        Error = "Khoản trả không có ảnh chứng minh",
                        Message = "",
                        Data = null
                    };
                }*/

                var paid = await paidDeptRepo.PaidDebtInEvent(paidParam);

                if (paidParam.IMGLinks.Any())
                    await imageRepo.AddIMGLinksDB("paidDept", paid.Id, paidParam.IMGLinks);


            }
            catch (Exception ex)
            {
                return new Respond<PaidDept>()
                {
                    StatusCode = HttpStatusCode.Accepted,
                    Error = "",
                    Message = "Inner exception: " + ex.InnerException.Message,
                    Data = null
                };
            }
            string message = "Tạo yêu cầu trả tiền thành công, đang chờ duyệt!";
            if (await memberRepo.IsOwner(paidParam.EventId, GetUserId()) ||
                await memberRepo.IsCashier(paidParam.EventId, GetUserId()))
                message = "Tạo yêu cầu trả tiền thành công!";
            return new Respond<PaidDept>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = message,
                Data = null
            };
        }

        // lấy code trả tiền
        [HttpGet("paid-code")]
        public async Task<Respond<string>> GetCodePaidDebt()
        {
            string code = "";
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            // vòng lặp check xem code đã tồn tại trong bảng paiddebt chưa
            while (true)
            {
                code = new string(Enumerable.Repeat(chars, 8)
                   .Select(s => s[random.Next(s.Length)]).ToArray());
                if (await paidDeptRepo.IsCodeExist(code) == false)// chưa tồn tại thì ok
                    break;
            }
            return new Respond<string>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Mã chứng từ thanh toán",
                Data = "BW_" + code
            };

        }

        // danh sách các yêu cầu đã xử lý: duyệt hoặc bị từ chối, cái này member ko thấy được
        [HttpGet("paid-handled/eventId={eventId}")]
        public async Task<Respond<List<DebtPaymentPending>>> PaidDebtHandled(int eventId)
        {
            bool isNormal = await memberRepo.IsNormalMember(eventId, GetUserId());
            if (isNormal)
                return new Respond<List<DebtPaymentPending>>()
                {
                    StatusCode = HttpStatusCode.NotAcceptable,
                    Error = "",
                    Message = "Chỉ chủ sở hoặc người kiểm duyệt mới xem được nội dung này!",
                    Data = null
                };
            bool isWaiting = false;
            List<DebtPaymentPending> list = await paidDeptRepo
                .PaidsWaitingOrHandled(GetUserId(), eventId, isWaiting);
            return new Respond<List<DebtPaymentPending>>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Yêu cầu trả tiền đã xử lý!",
                Data = list
            };
        }

        //danh sách các paiddebt đang chờ duyệt, member ko thấy được
        [HttpGet("paidSent-waiting/eventId={eventId}")]
        public async Task<Respond<List<DebtPaymentPending>>> PaidWaiting(int eventId)
        {
            bool isNormal = await memberRepo.IsNormalMember(eventId, GetUserId());
            if (isNormal)
                return new Respond<List<DebtPaymentPending>>()
                {
                    StatusCode = HttpStatusCode.NotAcceptable,
                    Error = "",
                    Message = "Chỉ chủ sở hoặc người kiểm duyệt mới xem được nội dung này!",
                    Data = null
                };
            bool isWaiting = true;
            List<DebtPaymentPending> list = await paidDeptRepo
                .PaidsWaitingOrHandled(GetUserId(), eventId, isWaiting);
            return new Respond<List<DebtPaymentPending>>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Yêu cầu trả tiền đang chờ duyệt!",
                Data = list
            };
        }

        // chấp thuận hoặc từ chối các yêu cầu trả tiền
        [HttpPost("paid-approve")]
        public async Task<Respond<string>> PaidDebtApprove(ListIdStatus list)
        {
            await paidDeptRepo.PaidDebtApprove(list, GetUserId());
            if (list.Status == 2)
                return new Respond<string>()
                {
                    StatusCode = HttpStatusCode.Accepted,
                    Error = "",
                    Message = "Đã phê duyệt các yêu cầu trả tiền!",
                    Data = "Đã phê duyệt các yêu cầu trả tiền!"
                };
            return new Respond<string>()
            {
                StatusCode = HttpStatusCode.NotAcceptable,
                Error = "",
                Message = "Đã từ chối các yêu cầu trả tiền!",
                Data = "Đã từ chối các yêu cầu trả tiền!"
            };
        }

        // xem chi tiết các yêu cầu trả tiền khi click vào
        [HttpGet("paid-detail/PaidId={paidid}")]
        public async Task<Respond<PaidDebtDetailScreen>> PaidDebtDetail(int paidid)
        {
            PaidDebtDetailScreen p = await paidDeptRepo.PaidDebtDetail(paidid);
            return new Respond<PaidDebtDetailScreen>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Chi tiết các yêu cầu trả tiền khi chọn.",
                Data = p
            };
        }

        // danh sách các yêu cầu trả tiền cá nhân mình đã gửi: cả 3 trạng thái:
        // đc duyệt, từ chối, đang chờ
        [HttpGet("paid-sent/EventId={eventId}")]
        public async Task<Respond<List<DebtPaymentPending>>> PaidSent(int eventId)
        {
            List<DebtPaymentPending> list = await paidDeptRepo
                .DebtSent(GetUserId(), eventId);
            return new Respond<List<DebtPaymentPending>>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Các yêu cầu trả tiền mình đã gửi trong sự kiện này.",
                Data = list
            };
        }

        // kiểm tra yêu cầu trả tiền trước đấy đã duyệt chưa, nếu chưa thì không được tạo thêm 
        // yêu cầu trả tiền
        [HttpGet("paid-check/EventId={eventId}")]
        public async Task<Respond<string>> PaidCheck(int eventId)
        {
            bool check = await paidDeptRepo.PaidCheck(eventId, GetUserId());
            if (check == false)
                return new Respond<string>()
                {
                    StatusCode = HttpStatusCode.NotAcceptable,
                    Error = "",
                    Message = "Bạn đã tạo yêu cầu trả tiền rồi, đang chờ duyệt!",
                    Data = "Bạn đã tạo yêu cầu trả tiền rồi, đang chờ duyệt!"
                };
            return new Respond<string>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Có thể tạo yêu cầu trả tiền!",
                Data = "Có thể tạo yêu cầu trả tiền!"
            };
        }
    }
}
