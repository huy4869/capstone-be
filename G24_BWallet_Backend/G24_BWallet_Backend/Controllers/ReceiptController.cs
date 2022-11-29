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

        public ReceiptController(IReceiptRepository InitReceiptRepo, IUserDeptRepository InitUserDeptRepo, IEventUserRepository InitEventUserRepo, IImageRepository InitImageRepo)
        {
            receiptRepo = InitReceiptRepo;
            userDeptRepo = InitUserDeptRepo;
            eventUserRepo = InitEventUserRepo;
            imageRepo = InitImageRepo;
        }
        protected int GetUserId()
        {
            return int.Parse(this.User.Claims.First(i => i.Type == "UserId").Value);
        }

        [HttpGet]
        public async Task<Respond<EventReceiptsInfo>> GetReceiptsByEventID([FromQuery] int eventid)
        {
            EventReceiptsInfo eventReceiptsInfo = await receiptRepo.GetEventReceiptsInfoAsync(eventid);

            if (eventReceiptsInfo == null)
            {
                return new Respond<EventReceiptsInfo>()
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Error = "lỗi không tìm thấy event",
                    Message = "",
                    Data = eventReceiptsInfo
                };
            }

            return new Respond<EventReceiptsInfo>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "lấy thông tin event và chính từ thành công",
                Data = eventReceiptsInfo
            };

        }


        [HttpGet("{receiptId}")]
        public async Task<Respond<ReceiptDetail>> GetReceipt(int receiptId)
        {
            var r = receiptRepo.GetReceiptByIDAsync(receiptId);

            if (r == null)
            {
                return new Respond<ReceiptDetail>()
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Error = "không tìm thấy hóa đơn",
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
                    Message = "tìm thấy hóa đơn",
                    Data = await r
                };
            }
        }


        //create receipt
        [HttpGet("create")]
        public async Task<Respond<List<Member>>> PrepareCreateReceipt([FromQuery] int EventID)
        {
            var eventUsers = eventUserRepo.GetAllEventUsersAsync(EventID);

            return new Respond<List<Member>>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "lấy danh sách thành viên trong event thành công",
                Data = await eventUsers
            };
        }

        [HttpPost("create")]
        public async Task<Respond<Receipt>> PostCreateReceipt([FromBody] ReceiptCreateParam receipt)
        {
            if (!receipt.IMGLinks.Any())
            {
                return new Respond<Receipt>()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Error = "hóa đơn không có ảnh chứng minh",
                    Message = "",
                    Data = null
                };
            }
            if (receipt.ReceiptAmount != receipt.UserDepts.Sum(ud => ud.Debt))
            {
                return new Respond<Receipt>()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Error = "hóa đơn không chia đúng với tổng",
                    Message = "",
                    Data = null
                };
            }

            var createReceiptTask = receiptRepo.AddReceiptAsync(receipt);

            Receipt createdReceipt = await createReceiptTask;

            await imageRepo.AddIMGLinksDB("receipt", createdReceipt.Id, receipt.IMGLinks);

            foreach (UserDept ud in receipt.UserDepts)
            {
                await userDeptRepo.AddUserDeptToReceiptAsync(ud, createdReceipt.Id);
            }

            createdReceipt.UserDepts = null;

            return new Respond<Receipt>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "tạo hóa đơn xong chờ chấp thuận",
                Data = createdReceipt
            };
        }


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

        [HttpGet("receipt-sent/EventId={eventId}")]
        public async Task<Respond<List<ReceiptSentParam>>> ReceiptSent(int eventId)
        {
            List<ReceiptSentParam> list = await receiptRepo.ReceiptsSent(GetUserId(), eventId, false);
            return new Respond<List<ReceiptSentParam>>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Các hoá đơn trong event này",
                Data = list
            };

        }

        [HttpGet("receipt-approve")]
        public async Task<Respond<string>> ReceiptApprove(ListIdStatus list)
        {
            await receiptRepo.PaidDebtApprove(list);
            return new Respond<string>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Đã phê duyệt hoặc từ chối các chứng từ",
                Data = null
            };

        }

        [HttpGet("receiptSent-waiting/eventId={eventId}")]
        public async Task<Respond<List<ReceiptSentParam>>> ReceiptSentWaiting(int eventId)
        {
            bool isWaiting = true;
            List<ReceiptSentParam> list = await receiptRepo.ReceiptsSent(GetUserId(), eventId, isWaiting);
            return new Respond<List<ReceiptSentParam>>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Danh sách chứng từ đang chờ xác nhân",
                Data = list
            };
        }
    }
}
