using Outbox.Web.Strategies;

namespace Outbox.Web.HostedServices;

public class ConnectService : IHostedService
{
    private readonly IServiceProvider provider;

    public ConnectService(IServiceProvider provider)
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

            var strategy = scope.ServiceProvider.GetRequiredService<ConnectStrategy>();

            try
            {
                await strategy.Forward();
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