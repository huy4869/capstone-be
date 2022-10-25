using G24_BWallet_Backend.Models;
using G24_BWallet_Backend.Models.ObjectType;
using G24_BWallet_Backend.Repository.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace G24_BWallet_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserDeptController : ControllerBase
    {
        private readonly IEventRepository repo;

        public UserDeptController(IEventRepository eventRepository)
        {
            repo = eventRepository;
        }
        // GET: api/<UserDeptController>
        [HttpGet]
        public async Task<Respond<IEnumerable<Receipt>>> GetAllUserDepts([FromBody] User user)//NOT DONE
        {
            //var events = repo.GetAllEventsAsync(user);
            return new Respond<IEnumerable<Receipt>>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Get event success",
                Data = null
            };
        }

        // GET api/<UserDeptController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<UserDeptController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<UserDeptController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<UserDeptController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
