using G24_BWallet_Backend.DBContexts;
using G24_BWallet_Backend.Models;
using G24_BWallet_Backend.Models.ObjectType;
using G24_BWallet_Backend.Repository;
using G24_BWallet_Backend.Repository.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;
using Twilio.TwiML.Voice;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace G24_BWallet_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReceiptController : ControllerBase
    {
        private readonly IReceiptRepository receiptRepo;
        private readonly IUserDeptRepository userDeptRepo;
        private readonly IEventUserRepository eventUserRepo;
        private readonly IEventRepository eventRepo;

        public ReceiptController(IReceiptRepository InitReceiptRepo, IUserDeptRepository InitUserDeptRepo, IEventUserRepository InitEventUserRepo, IEventRepository InitEventRepo)
        {
            receiptRepo = InitReceiptRepo;
            userDeptRepo = InitUserDeptRepo;
            eventUserRepo = InitEventUserRepo;
            eventRepo = InitEventRepo;
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
        public async Task<Respond<List<Receipt>>> GetReceipt(int receiptId)
        {
            var r = receiptRepo.GetReceiptByIDAsync(receiptId);

            if (r == null){
                return new Respond<List<Receipt>>()
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Error = "không tìm thấy hóa đơn",
                    Message = "",
                    Data = await r
                };
            } else {
                return new Respond<List<Receipt>>()
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
        public async Task<Respond<Receipt>> PostCreateReceipt([FromBody] Receipt receipt)
        {
            var createReceiptTask = receiptRepo.AddReceiptAsync(receipt);
            Receipt createdReceipt = await createReceiptTask;

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

        // PUT api/<ReceiptController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<ReceiptController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
