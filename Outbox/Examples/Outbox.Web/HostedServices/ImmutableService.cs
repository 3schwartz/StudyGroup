using Outbox.Web.Strategies;

namespace Outbox.Web.HostedServices
{
    public class ImmutableService : IHostedService
    {
        private readonly IServiceProvider provider;

        public ImmutableService(IServiceProvider provider)
        {
            this.provider = provider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(() => DoWorkAsync(cancellationToken), cancellationToken);

            return Task.CompletedTask;
        }

        public async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            using var scope = provider.CreateScope();

            var strategy = scope.ServiceProvider.GetRequiredService<ImmutableStrategy>();
            await strategy.Forward();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
