﻿using Capstone_API.Models;
using Capstone_API.Repository.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Capstone_API.Controllers
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

        //get all
        // GET: api/<ReceiptController>
        public async Task<Respond<IEnumerable<Receipt>>> GetAllReceipt()//NOT DONE
        {
            var r = receiptRepo.GetReceiptByIDAsync(1);
             
            return new Respond<IEnumerable<Receipt>>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Get event success",
                Data = await r
            };
        }


        [HttpGet("get_first")]
        public async Task<Respond<Receipt>> GetReceipt()//NOT DONE
        {
            var r = receiptRepo.GetReceiptByIDAsync2(1);

            return new Respond<Receipt>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Get event success",
                Data = r
            };
        }

        // GET api/<ReceiptController>/5
        [HttpGet("{id}")]
        public string Get(int id)//NOT DONE
        {
            return "value";
        }

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
                Data = new ArrayList { "name: " + receipt.ReceiptName + ",receiptUserID: " + receipt.User.UserID}
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
