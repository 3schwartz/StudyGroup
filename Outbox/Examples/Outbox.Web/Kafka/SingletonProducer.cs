using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Outbox.Web.Configurations;

namespace Outbox.Web.Kafka
{
    public class SingletonProducer : ISingletonProducer
    {
        private readonly IProducer<string, string> producer;

        public SingletonProducer(IOptions<KafkaOptions> kafkaOptions)
        {
            producer = new ProducerBuilder<string, string>(
                new ProducerConfig
                {
                    // Small improvement
                    //LingerMs = 1,
                    BootstrapServers = kafkaOptions.Value.BootstrapServer
                }).Build();
        }

        public IProducer<string, string> GetSingletonProducer()
        {
            return producer;
        }

        public void Dispose()
        {
            producer?.Dispose();
        }
    }
}
