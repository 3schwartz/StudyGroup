namespace RestServer.Models
{
    public static class StarWarsContextExtensions
    {
        public static void Seed(this StarWarsContext context)
        {
            if (!context.Droids.Any())
            {
                var newhope = new Episode {
                    Title = "NEWHOPE" 
                };
                var empire = new Episode { 
                    Title = "EMPIRE"
                };
                var jedi = new Episode { 
                    Title = "JEDI" 
                };
                var episodes = new List<Episode>{
                    newhope,
                    empire,
                    jedi,
                };

                context.Episodes.AddRange(episodes);
                context.SaveChanges();

                var threepio = new Droid
                {
                    Name = "C-3PO",
                     Episodes = new List<Episode>
                    {
                        newhope,
                        empire,
                        jedi
                    },
                    PrimaryFunction = "Protocol"
                };
                var artoo = new Droid
                {
                    Name = "R2-D2",
                    Episodes = new List<Episode>
                    {
                        newhope,
                        empire,
                        jedi
                    },
                    PrimaryFunction = "Astromech"
                };

                var droids = new List<Droid>
                {
                    threepio,
                    artoo
                };

                context.Droids.AddRange(droids);
                context.SaveChanges();
            }
        }
    }
}
