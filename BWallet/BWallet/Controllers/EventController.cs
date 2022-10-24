using BWallet.Models;
using BWallet.Models.ObjectType;
using BWallet.Repository.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace BWallet.Controllers
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
        public async Task<Respond<IEnumerable<Event>>> GetAllEvent([FromForm] int userID)
        {
            var events =  repo.GetAllEventsAsync(userID);
            return new Respond<IEnumerable<Event>>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Get event success",
                Data = await events
            };
        }

        [HttpPost]
        public async Task<Respond<int>> AddEvent([FromBody] NewEvent newEvent)
        {
            int eventID = await repo.AddEventAsync(newEvent.EventName, newEvent.EventDescript);
            await repo.AddEventMember(eventID, newEvent.Members);
            return new Respond<int>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Add event success",
                Data = eventID
            };
        }

    }
}
