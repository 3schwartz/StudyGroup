using HotChocolate.Subscriptions;
using Models;

namespace GraphQL
{
    public class Mutation
    {
        public async Task<DroidPayload> AddDroidAsync(
            AddDroidInput input,
            [Service]IDroidRepository droids,
            [Service]ITopicEventSender eventSender)
        {
            Droid droid = droids.Add(input.Name, input.PrimaryFunction);

            await eventSender.SendAsync(nameof(Subscription.OnDroidAddedAsync), droid);

            return new DroidPayload(droid);
        }
        
        public record AddDroidInput(string Name, string PrimaryFunction);

        public class DroidPayload
        {
            public Droid Droid { get; set; }
            public DroidPayload(Droid droid)
            {
                Droid = droid;
            }
        }
    }
}
