using G24_BWallet_Backend.Models.ObjectType;
using G24_BWallet_Backend.Models;
using G24_BWallet_Backend.Repository.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Collections;

namespace G24_BWallet_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaidDebtController : ControllerBase
    {
        private readonly IPaidDebtRepository repo;

        public PaidDebtController(IPaidDebtRepository repo)
        {
            this.repo = repo;
        }

        [HttpPost("listDebt")]
        public async Task<Respond<List<UserDebtReturn>>> GetListUserDebt([FromBody]EvenUserID e)
        {
            int status = 2;
            List<Receipt> receipt = await repo.GetReceipts(e.EventId, status);
            List<UserDebtReturn> userDepts = await repo.GetUserDepts(receipt,e.UserId);
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
        public async Task<Respond<string>> PaidDebt(PaidDebtParam p)
        {
            var paid = await repo.PaidDebtInEvent(p);
            return new Respond<string>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Danh sách các hoá đơn mình còn nợ trong event này",
                Data = paid
            };

        }

    }
}
