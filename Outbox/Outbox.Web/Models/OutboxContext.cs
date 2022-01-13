using Microsoft.EntityFrameworkCore;

namespace Outbox.Web.Models
{
    public class OutboxContext : DbContext
    {
        public OutboxContext(DbContextOptions<OutboxContext> options)
            : base(options) {
        }

        public DbSet<OutboxMessage> OutboxMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("outbox");
        }
    }
}
