using G24_BWallet_Backend.DBContexts;
using G24_BWallet_Backend.Models;
using G24_BWallet_Backend.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace G24_BWallet_Backend.Repository
{
    public class ProfileRepository : IProfileRepository
    {
        private readonly MyDBContext context;

        public ProfileRepository(MyDBContext myDB)
        {
            this.context = myDB;
        }

        // lấy các request mà nhóm mời mình vào,status = 0,1,2
        public async Task<List<Request>> GetInvitePending(int userId)
        {
            return await context.Requests.Where(r=>
            r.UserID == userId && (r.Status == 0 || r.Status == 1 || r.Status == 2))
                .ToListAsync();
        }

        // lấy các request mình xin vào nhóm, status = 3,4,5
        public async Task<List<Request>> GetRequestPending(int userId)
        {
            return await context.Requests.Where(r =>
            r.UserID == userId && (r.Status == 3 || r.Status == 4 || r.Status == 5))
                .ToListAsync();
        }

       
        public async Task<User> GetUserById(int userId)
        {
            return await context.Users.Include(u => u.Account)
                .FirstOrDefaultAsync(u => u.ID == userId);
        }
    }
}
