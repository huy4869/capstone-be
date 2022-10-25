using G24_BWallet_Backend.Models;
using G24_BWallet_Backend.Models.ObjectType;
using G24_BWallet_Backend.Repository.Interface;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IUserDeptRepository deptRepo;

        public ReceiptController(IReceiptRepository InitReceiptRepo, IUserDeptRepository InitUserDeptRepo)
        {
            receiptRepo = InitReceiptRepo;
            deptRepo = InitUserDeptRepo;
        }

        [HttpGet()]
        public async Task<Respond<IEnumerable<Receipt>>> GetReceiptByEventIDUserID([FromQuery(Name = "eventid")] int eventid, [FromQuery(Name = "userid")] int userid)
        {
            var receipts = receiptRepo.GetReceiptByEventIDUserIDAsync(eventid, userid) ;
            
            return new Respond<IEnumerable<Receipt>>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "lấy danh sách thành công",
                Data = await receipts
            };
        }


        [HttpGet("{id}")]
        public async Task<Respond<Receipt>> GetReceipt(int id)//NOT DONE
        {
            var r = receiptRepo.GetReceiptByIDAsync(id);

            return new Respond<Receipt>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Get receipt success",
                Data = r.Result
            };
        }
        /*
        // GET api/<ReceiptController>/5
        [HttpGet("{id}")]
        public string Get(int id)//NOT DONE
        {
            return "value";
        }*/

        //create receipt
        // POST api/<ReceiptController>
        [HttpPost("create")]
        public async Task<Respond<ArrayList>> CreateReceipt([FromForm] Receipt receipt /*List<User> involve_members = null, List<UserDept> userDepts = null*/)
        {//note all create will have status not approve

            //repo.createReceipt
            //switch(receipt)
            //foreach(userDepts)
            //deptRepo.createDept
            //end for
            return new Respond<ArrayList>()
            {
                StatusCode = HttpStatusCode.Created,
                Error = "",
                Message = "Testing",
                Data = new ArrayList { "name: " + receipt.ReceiptName + ",receiptUserID: " + receipt.User.ID}
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
