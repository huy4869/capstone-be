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

namespace G24_BWallet_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
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
            List<UserDebtReturn> userDepts = await paidDeptRepo.GetUserDepts(receipt,e.UserId);
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

    }
}
