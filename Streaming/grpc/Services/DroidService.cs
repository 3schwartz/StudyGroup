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
        private readonly TopicChannels channels;

        public DroidService(IDroidRepository droids, ILogger<DroidService> logger,
            TopicChannels channels)
        {
            this.droids = droids;
            this.logger = logger;
            this.channels = channels;
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

        public override async Task<GrpcService.Shared.Droid> AddDroid(GrpcService.Shared.Droid request, ServerCallContext context)
        {
            var droid = droids.Add(request.Name, request.PrimaryFunction);

            var grpcDroid = new GrpcService.Shared.Droid { Name = droid.Name, PrimaryFunction = droid.PrimaryFunction };

            logger.LogInformation("Adding droid to queue");

            await channels.TryWrite(nameof(DroidService.SubscribeDroidsAdded), grpcDroid);

            return grpcDroid;
        }

        public override async Task SubscribeDroidsAdded(Empty request, 
            IServerStreamWriter<DroidAddedResponse> responseStream,
            ServerCallContext context)
        {
            logger.LogInformation("Starting subscription");

            var channel = channels.InitSubscription(nameof(DroidService.SubscribeDroidsAdded));
            try
            {
                await foreach (var droid in channel.Reader.ReadAllAsync(context.CancellationToken))
                {
                    logger.LogInformation("Sending droid");

                    await responseStream.WriteAsync(new DroidAddedResponse
                    {
                        Droid = droid,
                        Timestamp = Timestamp.FromDateTime(DateTime.UtcNow)
                    });
                }
            }
            catch (OperationCanceledException ex)
            {
                logger.LogInformation(ex, "Operation canceled");
            }

            channels.CompleteSubscription(nameof(DroidService.SubscribeDroidsAdded));

            logger.LogInformation("Ending subscription");
        }
    }
}
