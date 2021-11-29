using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace PerformanceandMemoryLeak
{
    public class SomeWork : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(DoSomething, cancellationToken);


            return Task.CompletedTask;
        }

        public Task DoSomething()
        {
            var imCool = true;

            while (imCool)
            {
                // Do something
                Thread.Sleep(TimeSpan.FromSeconds(2));
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // Do nothing

            return Task.CompletedTask;
        }
    }
}
