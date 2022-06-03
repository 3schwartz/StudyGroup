namespace Models
{
    public class DroidRepository : IDroidRepository
    {
        private readonly IList<Droid> droids;

        public DroidRepository()
        {
            this.droids = new List<Droid> {
                new Droid {Name = "C-3PO", PrimaryFunction = "Protocol" },
                new Droid { Name = "R2-D2", PrimaryFunction = "Astromech" } 
            };
        }

        public Droid Add(string name, string primaryFunction)
        {
            var droid = new Droid { Name = name, PrimaryFunction = primaryFunction };

            droids.Add(droid);  

            return droid;
        }

        public IQueryable<Droid> GetDroids()
        {
            return droids.AsQueryable();
        }
    }
}
