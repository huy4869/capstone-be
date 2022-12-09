using G24_BWallet_Backend.Models;
using G24_BWallet_Backend.Models.ObjectType;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace G24_BWallet_Backend.Repository.Interface
{
    public interface IReceiptRepository
    {
        Task<ReceiptDetail> GetReceiptByIDAsync(int ReceiptID);

        Task<EventReceiptsInfo> GetEventReceiptsInfoAsync(int EventID, int userID);

        Task<Receipt> AddReceiptAsync(ReceiptCreateParam addReceipt, int userRole);
        Task<ReceiptUserDeptName> GetReceiptDetail(int receiptId);
        Task<List<ReceiptSentParam>> ReceiptsWaitingOrHandled(int userId, int eventId, bool isWaiting);
        Task ReceiptApprove(ListIdStatus list, int userId);
        Task<List<ReceiptSentParam>> ReceiptSent(int userId, int eventId);
    }
}
