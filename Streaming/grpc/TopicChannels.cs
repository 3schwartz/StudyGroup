using System.Threading.Channels;
using GrpcService.Shared;

namespace grpc
{
    public class TopicChannels
    {
        private readonly IDictionary<string, Channel<Droid>> subscriptions;

        public TopicChannels()
        {
            subscriptions = new Dictionary<string, Channel<Droid>>();
        }

        public async Task TryWrite(string channelName, Droid droid)
        {
            if(subscriptions.TryGetValue(channelName, out var channel))
            {
                await channel.Writer.WriteAsync(droid);
            }
        }

        public Channel<Droid> InitSubscription(string channelName)
        {
            var channel = Channel.CreateUnbounded<Droid>();

            subscriptions.Add(channelName, channel);

            return channel;
        }

        public void CompleteSubscription(string channelName)
        {
            if(subscriptions.Remove(channelName, out var channel))
            {
                channel.Writer.Complete();
            };
        }


    }
}
