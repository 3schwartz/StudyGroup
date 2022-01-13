using Confluent.Kafka;

namespace Outbox.Web.Factory
{
    public interface IConsumerFactory
    {
        IConsumer<string, string> GetConsumer(ConsumerConfig config, string topicToSubscribe);
    }
}
