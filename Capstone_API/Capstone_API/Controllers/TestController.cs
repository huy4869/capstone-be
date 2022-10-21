using Capstone_API.Repository.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Capstone_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly IAccessRepository repo;

        public TestController(IAccessRepository repo)
        {
            this.repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> Check([FromForm] string phone)
        {
            var check = repo.SendOtpTwilioAsync(phone,"Ko biet");
            return Ok(await check);
        }


    }
}
