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
        public async Task<Respond<JwtParam>> Login(Account acc)
        {
            var checkPhone = await repo.CheckPhoneNumberExistAsync(acc.PhoneNumber);
            //var encrypt = await repo.EncryptAsync(acc.Password);
            var encrypt = acc.Password;
            if (checkPhone == false)
                return new Respond<JwtParam>()
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Error = "Login fail!",
                    Message = "This phone number is not registed!",
                    Data = null
                };
            var account = await repo.GetAccountAsync(acc.PhoneNumber, encrypt);

            if (account == null)
                return new Respond<JwtParam>()
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Error = "Login fail!",
                    Message = "This password is wrong!",
                    Data = null
                };
            var user = await repo.GetUserAsync(account);
            var jwt = await repo.JWTGenerateAsync(acc.PhoneNumber, user.ID);
            var JWTToken = new JwtParam { access_token = jwt, type_token = "Bearer" };
            return new Respond<JwtParam>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Login success!",
                //Data = new ArrayList { new JWT(await jwt),await user}
                //Data = new Dictionary<object, object>()
                Data = JWTToken
                //{"Jwt" ,  jwt },
                //{nameof(user.ID), user.ID},
                //{nameof(user.UserName), user.UserName}
            };

        }
    }
}
