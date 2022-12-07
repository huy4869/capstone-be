using G24_BWallet_Backend.Models;
using G24_BWallet_Backend.Models.ObjectType;
using G24_BWallet_Backend.Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace G24_BWallet_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EventController : ControllerBase
    {
        private readonly IEventRepository repo;
        private readonly IMemberRepository repoMember;
        private readonly IReceiptRepository repoReceipt;
        private readonly IPaidDebtRepository repoPaidDebt;

        public EventController(IEventRepository eventRepository
            , IMemberRepository memberRepository
            , IReceiptRepository receiptRepository
            , IPaidDebtRepository paidDebtRepository)

        {
            repo = eventRepository;
            repoMember = memberRepository;
            repoReceipt = receiptRepository;
            repoPaidDebt = paidDebtRepository;
        }
        protected int GetUserId()
        {
            return int.Parse(this.User.Claims.First(i => i.Type == "UserId").Value);
        }

        [HttpGet]
        public async Task<Respond<IEnumerable<EventHome>>> GetAllEvent()
        {
            var events = repo.GetAllEventsAsync(GetUserId(), "");
            return new Respond<IEnumerable<EventHome>>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Get event success",
                Data = await events
            };
        }

        [HttpGet("search/name={name}")]
        public async Task<Respond<IEnumerable<EventHome>>> GetAllEventByName(string name)
        {
            var events = repo.GetAllEventsAsync(GetUserId(), name);
            return new Respond<IEnumerable<EventHome>>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Get event success",
                Data = await events
            };
        }

        [HttpGet("status/eventId={eventId}")]
        public async Task<Respond<IDictionary>> GetEventStatus(int eventId)
        {
            IDictionary status = await repo.GetEventStatus(eventId);
            return new Respond<IDictionary>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Event status: 1 Open _ 2 Close",
                Data = status
            };
        }

        [HttpPost]
        public async Task<Respond<string>> AddEvent(NewEvent newEvent)
        {
            newEvent.MemberIDs.Add(GetUserId());
            Event e = new Event
            {
                EventName = newEvent.EventName,
                EventDescript = newEvent.EventDescript,
                EventLogo = newEvent.EventLogo
            };
            int eventID = await repo.AddEventAsync(e);
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

        [HttpPost("join/eventId={eventId}")]
        public async Task<Respond<IDictionary>> CheckJoinByUrl(int eventId)
        {
            EventUserID eu = new EventUserID { EventId = eventId, UserId = GetUserId() };
            bool isJoin = await repo.CheckUserJoinEvent(eu);
            IDictionary<string, int> result = new Dictionary<string, int>
            {
                { "EventId", eventId }
            };
            if (isJoin == false)
                return new Respond<IDictionary>()
                {
                    StatusCode = HttpStatusCode.NotAcceptable,
                    Error = "",
                    Message = "User chưa tham gia event",
                    Data = (IDictionary)result
                };
            return new Respond<IDictionary>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "User đã tham gia event",
                Data = (IDictionary)result
            };
        }

        [HttpGet("ShareableLink/EventId={eventId}")]
        public async Task<Respond<string>> GetEventLink(int eventId)
        {
            string link = await repo.GetEventUrl(eventId);
            return new Respond<string>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Lấy link event đã tạo để share",
                Data = link
            };
        }

        [HttpGet("EventIntroduce/EventId={eventId}")]
        public async Task<Respond<IDictionary>> ShowEventIntroduce(int eventId)
        {
            Event e = await repo.GetEventById(eventId);
            List<UserAvatarName> u = await repo.GetListUserInEvent(eventId);
            IDictionary<string, object> result = new Dictionary<string, object>
            {
                { "EventLogo", e.EventLogo},
                {"EventName", e.EventName },
                {"EventDescript", e.EventDescript },
                {"TotalMembers", u.Count.ToString() },
                {"ListMembers", u }
            };
            return new Respond<IDictionary>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Lấy thông tin event và các thành viên để xin join",
                Data = (IDictionary)result
            };
        }

        [HttpPost("JoinRequest/EventId={eventId}")]
        public async Task<Respond<string>> SendJoinRequest(int eventId)
        {
            bool isMax = await repo.IsMaxMember(eventId);
            if(isMax)
                return new Respond<string>()
                {
                    StatusCode = HttpStatusCode.NotAcceptable,
                    Error = "",
                    Message = "",
                    Data = "Không thế tham gia được vì nhóm đã có đủ 500 thành viên"
                };
            EventUserID eventUserID = new EventUserID
            {
                EventId = eventId,
                UserId = GetUserId()
            };
            var check = await repo.SendJoinRequest(eventUserID);
            return new Respond<string>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "",
                Data = check
            };
        }

        //show thông tin event khi click vào tiêu đề
        [HttpGet("EventDetail/EventId={eventId}")]
        public async Task<Respond<IDictionary>> ShowEventDetail(int eventId)
        {
            int userId = GetUserId();
            Event e = await repo.GetEventById(eventId);
            List<UserAvatarName> u = await repo.GetListUserInEvent(eventId);
            var listJoinRequest = await repo.GetJoinRequest(eventId);
            IDictionary<string, object> result = new Dictionary<string, object>
            {
                { "EventLogo", e.EventLogo},
                {"EventName", e.EventName },
                {"EventDescript", e.EventDescript },
                {"TotalMembers", u.Count.ToString() },
            };
            if (await repoMember.IsOwner(eventId, userId))
            { // owner
                var listPaidDebtRequestSent = await repoPaidDebt.PaidDebtRequestSent(GetUserId(), eventId, false);
                var listReceiptSent = await repoReceipt.ReceiptsSent(GetUserId(), eventId, false);
                result.Add("JoinRequest", listJoinRequest.Count);
                result.Add("PaidDebtRequestSent", listPaidDebtRequestSent.Count);
                result.Add("ReceiptsSent", listReceiptSent.Count);
                result.Add("Role", 1);
            }
            else if (await repoMember.IsInspector(eventId, userId))
            { // inspector
                var receiptRequest = await repoReceipt.ReceiptsSent(GetUserId(), eventId, true);
                result.Add("ReceiptsWaiting", receiptRequest.Count);
                result.Add("Role", 2);
            }
            else if (await repoMember.IsCashier(eventId, userId))
            { // cashier
                var paidWaitConfirm = await repoPaidDebt.PaidWaitConfirm(eventId);
                result.Add("PaidRequestNumber", paidWaitConfirm.Count);
                result.Add("Role", 3);
            }
            else
            {
                // normal member
                var listPaidDebtRequestSent = await repoPaidDebt.PaidDebtRequestSent(GetUserId(), eventId, false);
                var listReceiptSent = await repoReceipt.ReceiptsSent(GetUserId(), eventId, false);
                result.Add("PaidDebtRequestSent", listPaidDebtRequestSent.Count);
                result.Add("ReceiptsSent", listReceiptSent.Count);
                result.Add("Role", 0);
            }
            return new Respond<IDictionary>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Lấy thông tin chi tiết trong event",
                Data = (IDictionary)result
            };
        }

        [HttpPost("edit-event")]
        public async Task<Respond<string>> EditEventInformation(EventIdNameDes e)
        {
            await repo.UpdateEventInformation(e);
            return new Respond<string>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Chỉnh sửa thông tin event thành công",
                Data = null
            };
        }

        [HttpPost("event-approve")]
        public async Task<Respond<string>> EventApprove(ListIdStatus list)
        {
            await repo.ApproveEventJoinRequest(list);
            return new Respond<string>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Xét duyệt tham gia vào event(đồng ý hoặc từ chối)",
                Data = null
            };
        }

        [HttpGet("joinRequest-history/EventId={eventId}")]
        public async Task<Respond<List<JoinRequestHistory>>> JoinRequestHistory(int eventId)
        {
            List<JoinRequestHistory> list = await repo.JoinRequestHistory(eventId);
            return new Respond<List<JoinRequestHistory>>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Lịch sử các request của event này",
                Data = list
            };
        }

        [HttpGet("joinRequest-waiting/EventId={eventId}")]
        public async Task<Respond<List<UserJoinRequestWaiting>>> JoinRequestWaiting(int eventId)
        {
            List<UserJoinRequestWaiting> list = await repo.GetJoinRequest(eventId);
            return new Respond<List<UserJoinRequestWaiting>>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Các request join đang chờ accept của event này",
                Data = list
            };
        }

        [HttpGet("event-close/EventId={eventId}")]
        public async Task<Respond<string>> CloseEvent(int eventId)
        {
            string result = await repo.CloseEvent(GetUserId(), eventId);
            if (result.Equals("Đóng event thành công") || result.Equals("Out event thành công"))
                return new Respond<string>()
                {
                    StatusCode = HttpStatusCode.Accepted,
                    Error = "",
                    Message = "",
                    Data = result
                };
            return new Respond<string>()
            {
                StatusCode = HttpStatusCode.NotAcceptable,
                Error = "",
                Message = "",
                Data = result
            };
        }
    }

}

