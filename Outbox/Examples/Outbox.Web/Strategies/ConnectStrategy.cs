using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Outbox.Web.Configurations;
using Outbox.Web.Factory;
using Outbox.Web.Kafka;
using Outbox.Web.Metrics;
using Outbox.Web.Models;
using Outbox.Web.Telemetry;

namespace Outbox.Web.Strategies
{
    public class ConnectStrategy : IOutboxStrategy
    {
        private const string ServiceName = "connect_strategy";

        private readonly KafkaOptions kafkaOptions;
        private readonly IConsumerFactory consumerFactory;
        private readonly CancellationTokenSource cts;
        private int iterationCount = 0;
        private int iterationMax = -1;

        private readonly IProducer<string, string> producer;

        public IConsumer<string, string> Consumer
        {
            get
            {
                consumer ??=
                    consumerFactory.GetConsumer(new ConsumerConfig
                    {
                        BootstrapServers = kafkaOptions.BootstrapServer,
                        GroupId = kafkaOptions.GroupId,
                        AutoOffsetReset = AutoOffsetReset.Earliest
                    },
                    kafkaOptions.ConnectTopic);
                return consumer;
            }
            set { consumer = value; }
        }

        private IConsumer<string, string>? consumer;


        public ConnectStrategy(IOptions<KafkaOptions> iKafkaOptions,
            IConsumerFactory consumerFactory, ISingletonProducer singletonProducer)
        {
            kafkaOptions = iKafkaOptions.Value;
            this.consumerFactory = consumerFactory;
            producer = singletonProducer.GetSingletonProducer();
            cts = new CancellationTokenSource();
        }

        /// <summary>
        /// Used to set a max amount of messages to be read.
        /// </summary>
        /// <param name="iterationMax"></param>
        public void SetIterationMax(int iterationMax)
        {
            this.iterationMax = iterationMax;
        }

        public async Task<bool> Forward()
        {
            while (iterationMax == -1 || iterationCount < iterationMax)
            {
                var consumeResult = Consumer.Consume(cts.Token);

                using var stateMetricHistogram = new StateMetricHistogram(ServiceName);
                using var activity = TelemetryTracing.TracingActivitySource.StartActivity(ServiceName);

                var connectMessage = JsonConvert.DeserializeObject<ConnectMessage>(consumeResult.Message.Value);

                if (connectMessage == null)
                {
                    var jsonSerializationException = new JsonSerializationException($"Couldn't Deserialize to ConnectMessage object {connectMessage}");

                    stateMetricHistogram.SetDoneWork(false);
                    stateMetricHistogram.SetError(jsonSerializationException);

                    activity?.SetTag("doneWork", false);

                    throw jsonSerializationException;
                }

                var messageToDeliver = new Message<string, string> { Value = JsonConvert.SerializeObject(connectMessage.ParseOutboxMessage()) };

                await producer.ProduceAsync(kafkaOptions.OutboxTopic, messageToDeliver);

                activity?.SetTag("doneWork", true);

                iterationCount++;
            }

            return true;            
        }
    }
}
