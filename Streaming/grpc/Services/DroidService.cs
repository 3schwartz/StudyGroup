using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcService.Shared;
using Models;

namespace grpc.Services
{
    public class DroidService : GrpcService.Shared.DroidService.DroidServiceBase
    {
        private readonly IDroidRepository droids;
        private readonly ILogger<DroidService> logger;
        private readonly Topics topics;

        public DroidService(IDroidRepository droids, ILogger<DroidService> logger, Topics topics)
        {
            this.droids = droids;
            this.logger = logger;
            this.topics = topics;
        }

        public override Task<DroidsResponse> GetDroids(Empty request, ServerCallContext context)
        {
            var response = new DroidsResponse
            {
                Droids = { }
            };
            
            foreach(var droid in droids.GetDroids())
            {
                response.Droids.Add(new GrpcService.Shared.Droid
                {
                    Name = droid.Name,
                    PrimaryFunction = droid.PrimaryFunction
                });
            }

            return Task.FromResult(response);
        }

        public override Task<GrpcService.Shared.Droid> AddDroid(GrpcService.Shared.Droid request, ServerCallContext context)
        {
            var droid = droids.Add(request.Name, request.PrimaryFunction);

            var grpcDroid = new GrpcService.Shared.Droid { Name = droid.Name, PrimaryFunction = droid.PrimaryFunction };

            logger.LogInformation("Adding droid to queue");

            topics.Enqueue(grpcDroid);

            return Task.FromResult(grpcDroid);
        }

        public override async Task SubscribeDroidsAdded(Empty request, 
            IServerStreamWriter<DroidAddedResponse> responseStream,
            ServerCallContext context)
        {
            logger.LogInformation("Starting subscription");

            do
            {
                if (topics.TryDequeue(out var droid))
                {
                    logger.LogInformation("Sending droid");

                    await responseStream.WriteAsync(new DroidAddedResponse
                    {
                        Droid = droid,
                        Timestamp = Timestamp.FromDateTime(DateTime.UtcNow)
                    });
                    continue;
                };

                await Task.Delay(100);
            } while (!context.CancellationToken.IsCancellationRequested);

            logger.LogInformation("Ending subscription");
        }
    }
}
