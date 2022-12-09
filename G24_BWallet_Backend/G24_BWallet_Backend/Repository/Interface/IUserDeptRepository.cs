using G24_BWallet_Backend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace G24_BWallet_Backend.Repository.Interface
{
    public interface IUserDeptRepository
    {
        Task<List<UserDept>> GeUserDeptByReceiptIDAsync(int ReceiptID);
        Task<List<UserDept>> GetReceiptByUserIDAsync(int UserID);
        Task<int> AddUserDeptToReceiptAsync(UserDept addUserDept, int receiptID, int userRole);
    }
}
