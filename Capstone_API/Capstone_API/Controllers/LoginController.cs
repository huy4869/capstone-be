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
    public class LoginController : ControllerBase
    {
        private readonly ILoginRepository _repo;
        private readonly IManagerRepository _mrepo;

        public LoginController(ILoginRepository repo, IManagerRepository mrepo)
        {
            _repo = repo;
            _mrepo = mrepo; 
        }

        // GET api/<LoginController>/5
        [HttpPost]
        public IActionResult Login([FromForm] string phone, [FromForm] string password)
        {
            if (_mrepo.CheckPhoneNumberExist(phone) == false)
                return NotFound("This phone number is not registed!");
            Account account = _repo.GetAccount(phone, password);
            if (account == null)
                return NotFound("This password is wrong!");
            return Ok("JWT Token: "+ _mrepo.JWTGenerate(phone,password));
        }
    }
}
