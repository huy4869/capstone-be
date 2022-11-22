using G24_BWallet_Backend.Models;
using G24_BWallet_Backend.Models.ObjectType;
using G24_BWallet_Backend.Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace G24_BWallet_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileRepository repo;

        public ProfileController(IProfileRepository repository)
        {
            repo = repository;
        }

        protected int GetUserId()
        {
            return int.Parse(this.User.Claims.First(i => i.Type == "UserId").Value);
        }

        [HttpGet]
        public async Task<Respond<IDictionary>> GetProfile()
        {
            var userID = GetUserId();
            User user = await repo.GetUserById(userID);
            List<Request> requestPending = await repo.GetRequestPending(userID);
            List<Request> invitePending = await repo.GetInvitePending(userID);
            IDictionary<string, object> dictionary = new Dictionary<string, object>();
            dictionary.Add("Avatar", user.Avatar);
            dictionary.Add("UserName", user.UserName);
            dictionary.Add("PhoneNumber", user.Account.PhoneNumber);
            dictionary.Add("RequestPending", requestPending.Count);
            dictionary.Add("InvitePending", invitePending.Count);
            return new Respond<IDictionary>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Lấy thông tin người dùng và yêu cầu tham gia, lời mời vào nhóm",
                Data = (IDictionary)dictionary
            };
        }
    }


}
