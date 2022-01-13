using Confluent.Kafka;
using Outbox.Web.Factory;

namespace Outbox.Web.Kafka
{
    public class ConsumerFactory : IConsumerFactory
    {
        public IConsumer<string,string> GetConsumer(ConsumerConfig config, string topicToSubscribe)
        {
            var consumer = new ConsumerBuilder<string, string>(config).Build();

            consumer.Subscribe(topicToSubscribe);

            return consumer;
        }
    }
}
