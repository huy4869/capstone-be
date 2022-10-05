using Capstone_API.DBContexts;
using Capstone_API.Models;
using Capstone_API.Repository.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Capstone_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // handle login proccess
    public class LoginController : ControllerBase
    {
        private readonly IAccessRepository _repo;

        public LoginController(IAccessRepository repo)
        {
            _repo = repo;
        }

        // GET api/<LoginController>/5
        [HttpPost]
        public IActionResult Login([FromForm] string phone, [FromForm] string password)
        {
            if (_repo.CheckPhoneNumberExist(phone) == false)
                return NotFound("This phone number is not registed!");
            Account account = _repo.GetAccount(phone, password);
            if (account == null)
                return NotFound("This password is wrong!");
            return Ok("JWT Token: "+ _repo.JWTGenerate(phone,password));
        }
    }
}
