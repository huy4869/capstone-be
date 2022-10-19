using Capstone_API.Models;
using Microsoft.EntityFrameworkCore;

namespace Capstone_API.DBContexts
{
    public class MyDBContext : DbContext
    {
        public DbSet<Account> Accounts { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Otp> Otps { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventUser> EventUsers { get; set; }
        public MyDBContext(DbContextOptions<MyDBContext> o) : base(o) { }

    }
}
