using Capstone_API.Models;
using Capstone_API.Repository.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Capstone_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly IEventRepository repo;

        public EventController(IEventRepository eventRepository)
        {
            repo = eventRepository;
        }

        [HttpGet]
        public async Task<Respond<IEnumerable<Event>>> GetAllEvent([FromBody] User user)
        {
            var events =  repo.GetAllEventsAsync(user);
            return new Respond<IEnumerable<Event>>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Get event success",
                Data = await events
            };
        }

        [HttpGet("user")]
        public async Task<IActionResult> GetAllUser()
        {
            return Ok(await repo.GetAllUsersAsync());
        }

    }
}
