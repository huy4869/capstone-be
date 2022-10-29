using G24_BWallet_Backend.Models;
using G24_BWallet_Backend.Models.ObjectType;
using G24_BWallet_Backend.Repository.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace G24_BWallet_Backend.Controllers
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

        [HttpGet("{userID}")]
        public async Task<Respond<IEnumerable<Event>>> GetAllEvent(int userID)
        {
            var events = repo.GetAllEventsAsync(userID);
            return new Respond<IEnumerable<Event>>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Get event success",
                Data = await events
            };
        }

        [HttpPost]
        public async Task<Respond<string>> AddEvent([FromBody] NewEvent newEvent)
        {
            int eventID = await repo.AddEventAsync(newEvent.Event);
            await repo.AddEventMember(eventID, newEvent.MemberIDs);
            string eventUrl = await repo.CreateEventUrl(eventID);
            return new Respond<string>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Add event success",
                Data = eventUrl
            };
        }

        [HttpPost("check-join")]
        public async Task<Respond<string>> CheckJoin(EventUser eu)
        {
            bool isJoin = await repo.CheckUserJoinEvent(eu);
            if (isJoin == false)
                return new Respond<string>()
                {
                    StatusCode = HttpStatusCode.Accepted,
                    Error = "",
                    Message = "You have NOT participated in the event",
                    Data = null
                };
            return new Respond<string>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "You have participated in the event",
                Data = null
            };
        }
    }

}

