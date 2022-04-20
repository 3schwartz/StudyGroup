using Microsoft.EntityFrameworkCore;

namespace Common.Models
{
    public class StarWarsContext : DbContext
    {
        public StarWarsContext(DbContextOptions<StarWarsContext> options)
            : base(options)
        {
        }

        public DbSet<Droid> Droids { get; set; }

        public DbSet<Episode> Episodes { get; set; }
    }
}
