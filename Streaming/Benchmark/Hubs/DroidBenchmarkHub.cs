using Microsoft.AspNetCore.SignalR;
using Models;

namespace Benchmark.Hubs
{
    public class DroidBenchmarkHub : Hub
    {
        internal static string DroidBenchmarkMethod = nameof(DroidBenchmark);
        
        public Task<Droid> DroidBenchmark()
        {
            return Task.FromResult(new Droid { Name = "R5-D4", PrimaryFunction = "Astromech" });
        }
    }
}
