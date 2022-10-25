using G24_BWallet_Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace G24_BWallet_Backend.DBContexts
{
    public class MyDBContext : DbContext
    {
        public DbSet<Account> Accounts { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Otp> Otps { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventUser> EventUsers { get; set; }
        public DbSet<Friend> Friends { get; set; }

        public DbSet<Receipt> Receipts { get; set; }

        public DbSet<UserDept> UserDepts { get; set; }
        public MyDBContext(DbContextOptions<MyDBContext> o) : base(o) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<EventUser>()
                .HasKey(eu => new { eu.EventID, eu.UserID });
            modelBuilder.Entity<Friend>()
               .HasKey(f => new { f.UserID, f.UserFriendID });
        }
    }
}
