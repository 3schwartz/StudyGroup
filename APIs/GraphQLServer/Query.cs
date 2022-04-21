using Common.Models;
using Microsoft.EntityFrameworkCore;

namespace GraphQLServer
{
    public class Query
    {
        public IQueryable<Droid> GetDroids([Service]StarWarsContext context) =>
            context.Droids.Include(d => d.Episodes);

        public IQueryable<Droid> GetDroidById(
            int id,
            [Service] StarWarsContext context) =>
            context.Droids.Where(d => d.Id == id).Include(d => d.Episodes);
    }
}
