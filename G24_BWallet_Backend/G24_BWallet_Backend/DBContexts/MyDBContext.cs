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
        public DbSet<PaidDebtList> PaidDebtLists { get; set; }
        public DbSet<PaidDept> PaidDepts { get; set; }
        public DbSet<ProofImage> ProofImages { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DbSet<Invite> Invites { get; set; }
        public DbSet<FAQ> FAQ { get; set; }
        public MyDBContext(DbContextOptions<MyDBContext> o) : base(o) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<EventUser>()
                .HasKey(eu => new { eu.EventID, eu.UserID });
            modelBuilder.Entity<Friend>()
               .HasKey(f => new { f.UserID, f.UserFriendID });
            //modelBuilder.Entity<Invite>()
            //   .HasKey(i => new { i.UserID , i.FriendId});
        }
    }
}
