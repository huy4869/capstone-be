using G24_BWallet_Backend.Models.ObjectType;
using System.Threading.Tasks;

namespace G24_BWallet_Backend.Repository.Interface
{
    public interface IDebtReceiveDetailRepo
    {
        Task<ReceiptUserDeptName> ClickIButton(int receiptId, int v);
        Task<TotalMoneyUser> GetAllDebtInEvent(int userId, int eventId);
        Task<TotalMoneyUser> GetAllReceiveInEvent(int v, int eventId);
        Task<string> SendRemind(IdAvatarNamePhoneMoney i);
    }
}
