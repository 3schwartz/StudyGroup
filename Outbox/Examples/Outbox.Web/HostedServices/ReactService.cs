using Outbox.Web.Strategies;

namespace Outbox.Web.HostedServices
{
    public class ReactService : IHostedService
    {
        private readonly IServiceProvider provider;

        public ReactService(IServiceProvider provider)
        {
            this.provider = provider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var task = Task.Run(async () =>
            {
                using var scope = provider.CreateScope();
                var reactiveStrategy = scope.ServiceProvider.GetRequiredService<ReactiveStrategy>();
                await reactiveStrategy.Forward();
            }, cancellationToken);

            return task;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
