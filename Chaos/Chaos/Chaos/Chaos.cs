using Polly.Contrib.Simmy;
using Polly.Contrib.Simmy.Outcomes;

namespace Chaos.Chaos
{
    public static class Chaos
    {
        public static AsyncInjectOutcomePolicy GetSomeChaos => MonkeyPolicy
            .InjectExceptionAsync(with =>
            with.Fault<Exception>(new Exception("Something"))
            .InjectionRate(0.9)
            .EnabledWhen((context, ct) => 
            Task.FromResult(Environment
                .GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                .Equals("Development"))));
    }
}
