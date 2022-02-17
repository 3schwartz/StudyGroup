namespace Chaos.Models
{
    public class Seed : BackgroundService
    {
        private readonly IServiceProvider provider;

        public Seed(IServiceProvider provider)
        {
            this.provider = provider;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = provider.CreateScope();

            var serviceProvider = scope.ServiceProvider;

            using var context = serviceProvider.GetRequiredService<ChaosContext>();

            context.Database.EnsureCreated();

            await context.ChaosItem.AddRangeAsync(new List<ChaosItem>
                {
                    new ChaosItem { Id = 1, Name = "Foo" },
                    new ChaosItem { Id = 2, Name = "Bar" }
                });
            
            await context.ChaosLinks.AddRangeAsync(new List<ChaosLink>
                {
                    new ChaosLink { Id = 1, Linked = 1 },
                    new ChaosLink { Id = 2, Linked = 1 },
                    new ChaosLink { Id = 3, Linked = 2 },
                    new ChaosLink { Id = 4, Linked = 2 },
                });
            
            await context.SaveChangesAsync();
        }
    }
}
