using Capstone_API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Capstone_API.Repository.Interface
{
    public interface IReceiptRepository
    {
        Task<Receipt> GetReceiptByIDAsync(int ReceiptID);

        Task<List<Receipt>> GetReceiptByEventIDUserIDAsync(int EventID, int UserID);
    }
}
