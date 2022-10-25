using G24_BWallet_Backend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace G24_BWallet_Backend.Repository.Interface
{
    public interface IReceiptRepository
    {
        Task<List<Receipt>> GetReceiptByIDAsync(int ReceiptID);

        Receipt GetReceiptByIDAsync2(int ReceiptID);


    }
}
