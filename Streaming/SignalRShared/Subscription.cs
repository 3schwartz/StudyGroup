using System.Threading.Channels;

namespace SignalRShared
{
    public class Subscription
    {
        private SubscriptionInfo? subscription;

        public void SetSubscription(SubscriptionInfo? info)
        {
            subscription = info;
        }

        public SubscriptionInfo? GetSubscriptionInfo()
        {
            return subscription;
        }
    }

    public record SubscriptionInfo(ChannelWriter<DroidAddedResponse> Writer, CancellationToken Ct);
}
