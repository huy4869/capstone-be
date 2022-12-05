using G24_BWallet_Backend.Models;
using G24_BWallet_Backend.Models.ObjectType;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace G24_BWallet_Backend.Repository.Interface
{
    public interface IPaidDebtRepository
    {
        Task<List<Receipt>> GetReceipts(int eventId, int status);
        Task<List<UserDebtReturn>> GetUserDepts(List<Receipt> receipt, int userId);
        Task PaidDebtApprove(ListIdStatus paid ,int userid);
        Task<PaidDept> PaidDebtInEvent(PaidDebtParam p);
        Task<List<DebtPaymentPending>> PaidDebtRequestSent(int userId, int eventId, bool isWaiting);
        Task<List<DebtPaymentPending>> PaidWaitConfirm(int eventId);
    }
}
