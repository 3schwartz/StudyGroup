using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace Benchmark
{
    public class BenchmarkBackgroundService : BackgroundService
    {
        private readonly IHostLifetime lifetime;

        public BenchmarkBackgroundService(IHostLifetime lifetime)
        {
            this.lifetime = lifetime;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await lifetime.WaitForStartAsync(stoppingToken);

            await Task.Run(() => BenchmarkRunner.Run<Benchmark>(DefaultConfig.Instance
                    .WithOptions(ConfigOptions.DisableOptimizationsValidator)));
        }
    }
}
