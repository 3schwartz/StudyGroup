using Microsoft.EntityFrameworkCore;

namespace Chaos.Models
{
    public class ChaosContext : DbContext
    {
        public ChaosContext(DbContextOptions<ChaosContext> options) : base(options)
        {
        }

        public DbSet<ChaosItem> ChaosItem { get; set; }
        public DbSet<ChaosLink> ChaosLinks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {}
    }

    public class ChaosItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class ChaosLink
    {
        public int Id { get; set; }
        public int Linked { get; set; }

    }
}
