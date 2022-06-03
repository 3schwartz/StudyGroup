using Models;

namespace GraphQL
{
    public class Query
    {
        public IQueryable<Droid> GetDroids([Service] IDroidRepository droids) => droids.GetDroids();
    }
}
