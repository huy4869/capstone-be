using G24_BWallet_Backend.DBContexts;
using G24_BWallet_Backend.Models;
using G24_BWallet_Backend.Models.ObjectType;
using G24_BWallet_Backend.Repository.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace G24_BWallet_Backend.Controllers
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
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Login page");
        }

        // GET api/<LoginController>/5
        [HttpPost]
        public async Task<Respond<IDictionary>> Login([FromBody] Account acc)
        {
            var checkPhone = repo.CheckPhoneNumberExistAsync(acc.PhoneNumber);
            //var encrypt = repo.EncryptAsync(password);
            var encrypt = acc.Password;
            if (await checkPhone == false)
                return new Respond<IDictionary>()
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Error = "Login fail!",
                    Message = "This phone number is not registed!",
                    Data = null
                };
            var account = repo.GetAccountAsync(acc.PhoneNumber, encrypt);
            var user = await repo.GetUserAsync(await account);
            if (await account == null)
                return new Respond<IDictionary>()
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Error = "Login fail!",
                    Message = "This password is wrong!",
                    Data = null
                };
            var jwt = await repo.JWTGenerateAsync(acc.PhoneNumber, encrypt);
            return new Respond<IDictionary>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Login success!",
                //Data = new ArrayList { new JWT(await jwt),await user}
                Data = new Dictionary<object, object>()
            {
                {"Jwt" ,  jwt },
                {nameof(user.ID), user.ID},
                {nameof(user.UserName), user.UserName}
            }
            };

        }
    }
}
