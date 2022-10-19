using Capstone_API.DBContexts;
using Capstone_API.Models;
using Capstone_API.Repository.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Capstone_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // handle login proccess
    public class LoginController : ControllerBase
    {
        private readonly IAccessRepository repo;

        public LoginController(IAccessRepository repo)
        {
            this.repo = repo;
        }

        // GET api/<LoginController>/5
        [HttpPost]
        public async Task<Respond<ArrayList>> Login([FromForm] string phone, [FromForm] string password)
        {
            var checkPhone = repo.CheckPhoneNumberExistAsync(phone);
            var encrypt = repo.EncryptAsync(password);
            if (await checkPhone == false)
                return new Respond<ArrayList>()
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Error = "Login fail!",
                    Message = "This phone number is not registed!",
                    Data = null
                };
            var account = repo.GetAccountAsync(phone, await encrypt);
            var user = repo.GetUserAsync(await account);
            if (await account == null)
                return new Respond<ArrayList>()
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Error = "Login fail!",
                    Message = "This password is wrong!",
                    Data = null
                };
            var jwt = repo.JWTGenerateAsync(phone, await encrypt);
            return new Respond<ArrayList>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Login success!",
                Data = new ArrayList { "JWT Token: " + await jwt, await account, await user}
            };

        }
    }
}
