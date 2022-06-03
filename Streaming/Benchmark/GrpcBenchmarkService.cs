using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcService.Bechmark;

internal class GrpcBenchmarkService : BechmarkService.BechmarkServiceBase
{
    public override Task<Droid> DroidBenchmark(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new Droid { Name = "R5-D4", PrimaryFunction = "Astromech" });
    }
}