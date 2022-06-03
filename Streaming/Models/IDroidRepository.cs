namespace Models
{
    public interface IDroidRepository
    {
        IQueryable<Droid> GetDroids();
        Droid Add(string name, string primaryFunction);
    }
}