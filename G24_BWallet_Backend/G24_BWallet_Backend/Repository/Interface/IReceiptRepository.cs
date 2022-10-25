using G24_BWallet_Backend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace G24_BWallet_Backend.Repository.Interface
{
    public interface IReceiptRepository
    {
        Task<Receipt> GetReceiptByIDAsync(int ReceiptID);

        Task<List<Receipt>> GetReceiptByEventIDUserIDAsync(int EventID, int UserID);
    }
}
