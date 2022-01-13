using Microsoft.EntityFrameworkCore;

namespace Outbox.Web.Models
{
    public class OutboxContext : DbContext
    {
        public OutboxContext(DbContextOptions<OutboxContext> options)
            : base(options) {
        }

        public DbSet<OutboxMessage> OutboxMessages { get; set; }
        public DbSet<OutboxMessageBatch> OutboxMessageBatches { get; set; }
        public DbSet<OutboxMessageConnect> OutboxMessageConnect { get; set; }
        public DbSet<OutboxMessageConcurrent> OutboxMessageConcurrent { get; set; }

        public DbSet<OutboxMessageImmutable> OutboxMessageImmutables { get; set; }
        public DbSet<OutboxMessageReact> OutboxMessageReact { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("outbox");
        }
    }
}
