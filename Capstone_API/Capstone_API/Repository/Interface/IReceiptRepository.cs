using Capstone_API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Capstone_API.Repository.Interface
{
    public interface IReceiptRepository
    {
        Task<List<Receipt>> GetReceiptByIDAsync(int ReceiptID);

        Receipt GetReceiptByIDAsync2(int ReceiptID);


    }
}
