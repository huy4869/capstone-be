using G24_BWallet_Backend.Models.ObjectType;
using G24_BWallet_Backend.Repository.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace G24_BWallet_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MemberController : ControllerBase
    {
        private readonly IMemberRepository repo;

        public MemberController(IMemberRepository member)
        {
            repo = member;
        }
        protected int GetUserId()
        {
            return int.Parse(this.User.Claims.First(i => i.Type == "UserId").Value);
        }

        // show ra các thành viên trong event, ai là kiểm duyệt, thu ngân, member
        [HttpGet("eventId={eventId}")]
        public async Task<Respond<MemberDetailParam>> ShowAllMemeberAndRole(int eventId)
        {
            MemberDetailParam m = await repo.ShowMemeberDetail(eventId);
            return new Respond<MemberDetailParam>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Show ra các thành viên trong event, " +
                "ai là kiểm duyệt, thu ngân, member",
                Data = m
            };
        }
    }
}
