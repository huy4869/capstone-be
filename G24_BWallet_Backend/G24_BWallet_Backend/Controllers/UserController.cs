using G24_BWallet_Backend.Models;
using G24_BWallet_Backend.Models.ObjectType;
using G24_BWallet_Backend.Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace G24_BWallet_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IAccessRepository accessRepo;

        public UserController(IAccessRepository repo)
        {
            this.accessRepo = repo;
        }
        protected int GetUserId()
        {
            return int.Parse(this.User.Claims.First(i => i.Type == "UserId").Value);
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            return Ok(GetUserId()/*await accessRepo.GetAllUserAsync()*/);
        }


        [HttpPost]
        public async Task<Respond<string>> EditUserProfile(User userEditInfo)
        {
            await accessRepo.UpdateUserProfile(userEditInfo,GetUserId());

            return new Respond<string>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Chỉnh sửa thông tin user thành công",
                Data = null
            };
        }
    }
}
