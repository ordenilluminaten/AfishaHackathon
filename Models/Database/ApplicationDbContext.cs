using Microsoft.EntityFrameworkCore;
using Models.Database.Tables;

namespace Models {
    public class ApplicationDbContext : DbContext {
        public ApplicationDbContext(DbContextOptions options) : base(options) {

        }

        public virtual DbSet<UserEvent> UserEvents { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserEventNotification> UserEventNotifications { get; set; }
        public virtual DbSet<UserNotification> UserNotifications { get; set; }

        protected override void OnModelCreating(ModelBuilder builder) {
            base.OnModelCreating(builder);
        }
    }
}
