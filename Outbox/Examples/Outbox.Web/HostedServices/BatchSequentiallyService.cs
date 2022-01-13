using Outbox.Web.Strategies;

namespace Outbox.Web.HostedServices
{
    public class BatchSequentiallyService : IHostedService
    {
        private const int BatchSize = 20;
        private readonly IServiceProvider provider;

        public BatchSequentiallyService(IServiceProvider provider)
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
            while (!cancellationToken.IsCancellationRequested)
            {
                using var scope = provider.CreateScope();

                var strategy = scope.ServiceProvider.GetRequiredService<SequentiallyStrategy>();
                strategy.SetBatchSize(BatchSize);

                try
                {
                    var didSomeWork = await strategy.Forward();
                    if (!didSomeWork) Thread.Sleep(5000);
                }
                catch (Exception e)
                {
                    // swallow
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
