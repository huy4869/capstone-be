using BWallet.Repository.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BWallet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IAccessRepository repo;

        public UserController(IAccessRepository repo)
        {
            this.repo = repo;
        }
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            return Ok(await repo.GetAllUserAsync());
        }
    }
}
