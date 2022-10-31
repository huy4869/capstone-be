using G24_BWallet_Backend.DBContexts;
using G24_BWallet_Backend.Models;
using G24_BWallet_Backend.Models.ObjectType;
using G24_BWallet_Backend.Repository;
using G24_BWallet_Backend.Repository.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;

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

        public ReceiptController(IReceiptRepository InitReceiptRepo, IUserDeptRepository InitUserDeptRepo, IEventUserRepository InitEventUserRepo)
        {
            receiptRepo = InitReceiptRepo;
            userDeptRepo = InitUserDeptRepo;
            eventUserRepo = InitEventUserRepo;
        }

        [HttpGet]
        public async Task<Respond<IEnumerable<Receipt>>> GetReceiptByEventID([FromQuery] int eventid)
        {
            //nếu nhập link "api/receipt" báo thiếu trường tìm kiếm
            if (String.IsNullOrEmpty(eventid.ToString()))
                return new Respond<IEnumerable<Receipt>>()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Error = "thiếu trường tìm kiếm",
                    Message = "",
                    Data = null
                };

            //thực hiện tìm kiếm theo event id
            var receipts = receiptRepo.GetReceiptByEventIDAsync(eventid) ;
            if (receipts.Result.IsNullOrEmpty()) {
                return new Respond<IEnumerable<Receipt>>()
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Error = "không tìm thấy các hóa đơn sự kiện",
                    Message = "",
                    Data = await receipts
                };
            }
            else
            {
                return new Respond<IEnumerable<Receipt>>()
                {
                    StatusCode = HttpStatusCode.OK,
                    Error = "",
                    Message = "lấy danh sách hóa đơn thành công",
                    Data = await receipts
                };
            }
        }


        [HttpGet("{eventId}")]
        public async Task<Respond<List<Receipt>>> GetReceipt(int eventId)
        {

            var r = receiptRepo.GetReceiptByEventIDAsync(1);

            if(r == null){
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
                    StatusCode = HttpStatusCode.OK,
                    Error = "",
                    Message = "tìm thấy hóa đơn",
                    Data = await r
                };
            }
        }


        //create receipt
        [HttpGet("create")]
        public async Task<Respond<List<Member>>> PrepareCreateReceipt([FromQuery] string eventid)
        {
            int CheckEventID;
            bool isNumeric = int.TryParse(eventid, out CheckEventID);
            if (!isNumeric)
            {
                return new Respond<List<Member>>()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Error = "sai event id",
                    Message = "",
                    Data = null
                };
            }

            var eventUsers = eventUserRepo.GetAllEventUsersAsync(CheckEventID);

            return new Respond<List<Member>>()
            {
                StatusCode = HttpStatusCode.OK,
                Error = "",
                Message = "lấy danh sách thành viên trong event thành công",
                Data = await eventUsers
            };
        }

        [HttpPost("create")]
        public async Task<Respond<int>> PostCreateReceipt([FromBody] Receipt receipt)
        {//note all create will have status not approve
            //repo.createReceipt
            var createReceiptTask = receiptRepo.AddReceiptAsync(receipt);
            int createReceiptId = await createReceiptTask;
            //
            //foreach(users)
            //deptRepo.createDept
            //end for
            foreach (UserDept ud in receipt.listUserDept)
            {
                await userDeptRepo.AddUserDeptToReceiptAsync(ud, createReceiptId);
            }

            return new Respond<int>()
            {
                StatusCode = HttpStatusCode.Created,
                Error = "",
                Message = "tạo hóa đơn xong chờ chấp thuận",
                Data = await createReceiptTask
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
