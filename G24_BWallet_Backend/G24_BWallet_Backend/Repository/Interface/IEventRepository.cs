﻿using G24_BWallet_Backend.Models;
using G24_BWallet_Backend.Models.ObjectType;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace G24_BWallet_Backend.Repository.Interface
{
    public interface IEventRepository
    {
        Task<List<EventHome>> GetAllEventsAsync(int userID, string name);
        Task<int> AddEventAsync(Event e, int userId);
        Task AddEventMember(int eventID, List<int> memebers);
        Task<string> CreateEventUrl(int eventID);
        Task<bool> CheckUserJoinEvent(EventUserID eu);
        Task<string> GetEventUrl(int eventId);
        Task<Event> GetEventById(int eventId);
        Task<List<UserAvatarName>> GetListUserInEvent(int eventId, bool getAll);
        Task<string> SendJoinRequest(EventUserID eventUserID);
        Task<List<UserJoinRequestWaiting>> GetJoinRequest(int eventId);
        Task UpdateEventInformation(EventIdNameDes e);
        Task ApproveEventJoinRequest(ListIdStatus list, int userId);
        Task<List<JoinRequestHistory>> JoinRequestHistory(int eventId);
        Task<string> CloseEvent(int v, int eventId);
        Task<bool> IsMaxMember(int eventId);
        Task<IDictionary> GetEventStatus(int eventId);
        Task<List<Report>> GetReportWaiting(int eventId);
        Task<NumberMoney> GetReceiveMoney(int eventId, int userID);
        Task<NumberMoney> GetDebtMoney(int eventId, int userID);
        Task<int> GetEventIdByUrl(string eventId);
    }
}
