using G24_BWallet_Backend.Models.ObjectType;
using G24_BWallet_Backend.Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace G24_BWallet_Backend.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class ActivityController : ControllerBase
    {
        private readonly IActivityRepository repo;

        public ActivityController(IActivityRepository activityRepository)

        {
            repo = activityRepository;
        }
        protected int GetUserId()
        {
            return int.Parse(this.User.Claims.First(i => i.Type == "UserId").Value);
        }


        // lấy tất cả các activity của mình 
        [HttpGet]
        public async Task<Respond<List<ActivityScreen>>> GetActivity()
        {
            List<ActivityScreen> list = await repo.GetActivity(GetUserId());
            return new Respond<List<ActivityScreen>>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Các hoạt động của mình.",
                Data = list
            };
        }

    }
}
