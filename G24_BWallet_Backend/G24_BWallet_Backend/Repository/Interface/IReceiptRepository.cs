using G24_BWallet_Backend.Models;
using G24_BWallet_Backend.Models.ObjectType;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace G24_BWallet_Backend.Repository.Interface
{
    public interface IReceiptRepository
    {
        Task<List<Receipt>> GetReceiptByIDAsync(int ReceiptID);

        Task<EventReceiptsInfo> GetEventReceiptsInfoAsync(int EventID);

        Task<Receipt> AddReceiptAsync(Receipt addReceipt);
    }
}
