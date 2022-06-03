using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Models;
using System.Threading.Channels;

namespace SignalRShared.Hubs
{
    public class DroidHub : Hub
    {
        private readonly IDroidRepository droids;
        private readonly ILogger<DroidHub> logger;
        private readonly Subscription subscription;

        public DroidHub(IDroidRepository droids, ILogger<DroidHub> logger, Subscription subscription)
        {
            this.droids = droids;
            this.logger = logger;
            this.subscription = subscription;
        }

        public Task<List<Droid>> GetDroids()
        {
            logger.LogInformation("Recieved request for droids");

            return Task.FromResult(droids.GetDroids().ToList());
        }

        public async Task AddDroids(Droid droid)
        {
            var newDroid = droids.Add(droid.Name, droid.PrimaryFunction);

            await Notify(droid);

            await Clients.All.SendAsync(Contracts.AddedDroid, newDroid);
        }

        public ChannelReader<DroidAddedResponse> SubscribeDroidsAdded(CancellationToken ct)
        {
            logger.LogInformation("Subscription started");

            var channel = Channel.CreateUnbounded<DroidAddedResponse>();

            subscription.SetSubscription(new SubscriptionInfo(channel.Writer, ct));

            return channel.Reader;
        }

        private async Task Notify(Droid droid)
        {
            var info = subscription.GetSubscriptionInfo();
            if (info == null) return;

            if (info.Ct.IsCancellationRequested)
            {
                info.Writer.Complete();
                subscription.SetSubscription(null);
                return;
            }

            logger.LogInformation("Notify subscriber");

            await info.Writer.WriteAsync(new DroidAddedResponse(DateTime.UtcNow, droid));
        }
    }
}
