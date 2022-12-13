using G24_BWallet_Backend.Models;
using G24_BWallet_Backend.Models.ObjectType;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace G24_BWallet_Backend.Repository.Interface
{
    public interface IPaidDebtRepository
    {
        Task<List<DebtPaymentPending>> DebtSent(int userId, int eventId);
        Task<List<Receipt>> GetReceipts(int eventId, int status);
        Task<List<UserDebtReturn>> GetUserDepts(List<Receipt> receipt, int userId);
        Task<bool> PaidCheck(int eventId, int userId);
        Task PaidDebtApprove(ListIdStatus paid ,int userid);
        Task<PaidDebtDetailScreen> PaidDebtDetail(int paidid);
        Task<PaidDept> PaidDebtInEvent(PaidDebtParam p);
        Task<List<DebtPaymentPending>> PaidsWaitingOrHandled(int userId, int eventId, bool isWaiting);
        Task<List<DebtPaymentPending>> PaidWaitConfirm(int eventId);
        
    }
}
