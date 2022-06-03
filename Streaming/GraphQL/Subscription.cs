using Models;

namespace GraphQL
{
    public class Subscription
    {
        [Subscribe]
        [Topic]
        public Task<DroidCreated> OnDroidAddedAsync(
            [EventMessage]Droid droid)
        {
            return Task.FromResult(new DroidCreated(droid, DateTime.UtcNow));
        }

        public record DroidCreated(Droid Droid, DateTime Timestamp);
    }
}
