using G24_BWallet_Backend.DBContexts;
using G24_BWallet_Backend.Models;
using G24_BWallet_Backend.Models.ObjectType;
using G24_BWallet_Backend.Repository;
using G24_BWallet_Backend.Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;
using Twilio.TwiML.Voice;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace G24_BWallet_Backend.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]

    public class ReceiptController : ControllerBase
    {
        private readonly IReceiptRepository receiptRepo;
        private readonly IUserDeptRepository userDeptRepo;
        private readonly IEventUserRepository eventUserRepo;
        private readonly IImageRepository imageRepo;
        private readonly IMemberRepository memberRepo;

        public ReceiptController(IReceiptRepository InitReceiptRepo, IUserDeptRepository InitUserDeptRepo, IEventUserRepository InitEventUserRepo, IImageRepository InitImageRepo, IMemberRepository InitMemberRepo)
        {
            receiptRepo = InitReceiptRepo;
            userDeptRepo = InitUserDeptRepo;
            eventUserRepo = InitEventUserRepo;
            imageRepo = InitImageRepo;
            memberRepo = InitMemberRepo;
        }
        protected int GetUserId()
        {
            return int.Parse(this.User.Claims.First(i => i.Type == "UserId").Value);
        }

        // lấy tất cả receipt trong event này
        [HttpGet]
        public async Task<Respond<EventReceiptsInfo>> GetReceiptsByEventID([FromQuery] int eventid)
        {
            int userID = GetUserId();
            EventReceiptsInfo eventReceiptsInfo =
                await receiptRepo.GetEventReceiptsInfoAsync(eventid, userID);

            if (eventReceiptsInfo == null)
            {
                return new Respond<EventReceiptsInfo>()
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Error = "Không tìm thấy sự kiện!",
                    Message = "",
                    Data = eventReceiptsInfo
                };
            }

            return new Respond<EventReceiptsInfo>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Lấy thông tin sự kiện và các chứng từ thành công!",
                Data = eventReceiptsInfo
            };
        }


        /*[HttpGet("{receiptId}")]
        public async Task<Respond<ReceiptDetail>> GetReceipt(int receiptId)
        {
            var r = receiptRepo.GetReceiptByIDAsync(receiptId);

            if (r == null)
            {
                return new Respond<ReceiptDetail>()
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Error = "không tìm thấy chứng từ",
                    Message = "",
                    Data = await r
                };
            }
            else
            {
                return new Respond<ReceiptDetail>()
                {
                    StatusCode = HttpStatusCode.Accepted,
                    Error = "",
                    Message = "tìm thấy chứng từ",
                    Data = await r
                };
            }
        }*/


        //lấy danh sách thành viên trong event
        [HttpGet("create")]
        public async Task<Respond<List<Member>>> PrepareCreateReceipt([FromQuery] int EventID, string name)
        {

            var eventUsers = eventUserRepo.SearchEventUsersAsync(EventID, GetUserId(), name);

            return new Respond<List<Member>>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Lấy danh sách thành viên trong sự kiện thành công!",
                Data = await eventUsers
            };
        }

        // tạo chứng từ 
        [HttpPost("create")]
        public async Task<Respond<Receipt>> PostCreateReceipt([FromBody] ReceiptCreateParam receipt)
        {
            int userID = GetUserId();
            if (receipt.ReceiptName.IsNullOrEmpty() || receipt.ReceiptAmount <= 0)
            {
                return new Respond<Receipt>()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Error = "Chứng từ thiếu thông tin!",
                    Message = "",
                    Data = null
                };
            }
            if (!receipt.IMGLinks.Any())
            {
                return new Respond<Receipt>()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Error = "Chứng từ không có ảnh chứng minh!",
                    Message = "",
                    Data = null
                };
            }

            if (receipt.ReceiptAmount < receipt.UserDepts.Sum(ud => ud.Debt))
            {
                return new Respond<Receipt>()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Error = "Chia không đúng số tiền tổng!",
                    Message = "",
                    Data = null
                };
            }

            receipt.UserID = userID;
            int userRole = await eventUserRepo.GetEventUserRoleAsync(receipt.EventID, receipt.UserID);
            var createReceiptTask = receiptRepo.AddReceiptAsync(receipt, userRole);

            Receipt createdReceipt = await createReceiptTask;

            await imageRepo.AddIMGLinksDB("receipt", createdReceipt.Id, receipt.IMGLinks);

            foreach (UserDept ud in receipt.UserDepts)
            {
                //ud.Debt = (int)(ud.Debt / 1);
                if (ud.UserId != userID)
                    await userDeptRepo.AddUserDeptToReceiptAsync(ud, createdReceipt.Id, userRole);
            }

            createdReceipt.UserDepts = null;
            string message = "Tạo chứng từ thành công, đang chờ duyệt!";
            if (await memberRepo.IsOwner(receipt.EventID, userID) ||
                await memberRepo.IsInspector(receipt.EventID, userID))
                message = "Tạo chứng từ thành công!";
            return new Respond<Receipt>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = message,
                Data = createdReceipt
            };
        }

        // lấy thông tin chi tiết của receipt khi click vào 
        [HttpGet("receipt-detail/ReceiptId={receiptId}")]
        public async Task<Respond<ReceiptUserDeptName>> ReceiptDetail(int receiptId)
        {
            ReceiptUserDeptName list = await receiptRepo.GetReceiptDetail(receiptId);
            return new Respond<ReceiptUserDeptName>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Thông tin chi tiết về hoá đơn này",
                Data = list
            };

        }

        // đồng ý hay từ chối receipt
        [HttpPost("receipt-approve")]
        public async Task<Respond<string>> ReceiptApprove(ListIdStatus list)
        {
            await receiptRepo.ReceiptApprove(list, GetUserId());
            if (list.Status == 2)
                return new Respond<string>()
                {
                    StatusCode = HttpStatusCode.Accepted,
                    Error = "",
                    Message = "",
                    Data = "Chứng từ đã được duyệt"
                };
            return new Respond<string>()
            {
                StatusCode = HttpStatusCode.NotAcceptable,
                Error = "",
                Message = "",
                Data = "Chứng từ đã bị từ chối"
            };
        }

        // danh sách các chứng từ đang chờ xử lý, cái này member ko thấy được
        [HttpGet("receiptSent-waiting/eventId={eventId}")]
        public async Task<Respond<List<ReceiptSentParam>>> ReceiptSentWaiting(int eventId)
        {
            bool isNormal = await memberRepo.IsNormalMember(eventId, GetUserId());
            if (isNormal)
                return new Respond<List<ReceiptSentParam>>()
                {
                    StatusCode = HttpStatusCode.NotAcceptable,
                    Error = "",
                    Message = "Chỉ chủ sở hoặc người kiểm duyệt mới xem được nội dung này!",
                    Data = null
                };
            bool isWaiting = true;
            List<ReceiptSentParam> list = await receiptRepo
                .ReceiptsWaitingOrHandled(GetUserId(), eventId, isWaiting);
            return new Respond<List<ReceiptSentParam>>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Danh sách chứng từ đang chờ xác nhân",
                Data = list
            };
        }

        // danh sách lịch sử các chứng từ đã xử lý: duyệt hoặc bị từ chối, cái này member ko thấy được
        [HttpGet("receipt-handled/EventId={eventId}")]
        public async Task<Respond<List<ReceiptSentParam>>> ReceiptHandled(int eventId)
        {
            bool isNormal = await memberRepo.IsNormalMember(eventId, GetUserId());
            if (isNormal)
                return new Respond<List<ReceiptSentParam>>()
                {
                    StatusCode = HttpStatusCode.NotAcceptable,
                    Error = "",
                    Message = "Chỉ chủ sở hoặc người kiểm duyệt mới xem được nội dung này!",
                    Data = null
                };
            List<ReceiptSentParam> list = await receiptRepo
                .ReceiptsWaitingOrHandled(GetUserId(), eventId, false);
            return new Respond<List<ReceiptSentParam>>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Các chứng từ đã xử lý trong event này",
                Data = list
            };
        }

        // danh sách các chứng từ mà cá nhân mình đã gửi: cả 3 trạng thái: đc duyệt, từ chối, đang chờ
        [HttpGet("receipt-sent/EventId={eventId}")]
        public async Task<Respond<List<ReceiptSentParam>>> ReceiptSent(int eventId)
        {
            List<ReceiptSentParam> list = await receiptRepo
                .ReceiptSent(GetUserId(), eventId);
            return new Respond<List<ReceiptSentParam>>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Các chứng từ đã gửi trong event này",
                Data = list
            };
        }
    }
}
