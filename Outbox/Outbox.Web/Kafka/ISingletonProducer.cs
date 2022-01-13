using Confluent.Kafka;

namespace Outbox.Web.Kafka
{
    public interface ISingletonProducer : IDisposable
    {
        IProducer<string, string> GetSingletonProducer();
    }
}
