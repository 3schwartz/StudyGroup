using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Outbox.Web.Configurations;
using Outbox.Web.Models;
using System.Text.Json;
using Outbox.Web.Kafka;
using Outbox.Web.Metrics;
using Outbox.Web.Telemetry;
using Prometheus;

namespace Outbox.Web.Strategies
{
    public class SequentiallyStrategy : IOutboxStrategy
    {
        private const string ServiceName = "sequentially_strategy";

        private readonly OutboxContext context;
        private readonly KafkaOptions kafkaOptions;
        private readonly IProducer<string, string> producer;

        private int batchSize = 1;

        public SequentiallyStrategy(OutboxContext context,
            IOptions<KafkaOptions> kafkaOptions,
            ISingletonProducer singletonProducer)
        {
            this.context = context;
            this.kafkaOptions = kafkaOptions.Value;
            producer = singletonProducer.GetSingletonProducer();
        }

        /// <summary>
        /// Use this setter to specify which kind of Sequentially Strategy it is.
        /// When NOT 1 then it is target Batch table.
        /// </summary>
        /// <param name="batchSize"></param>
        public void SetBatchSize(int batchSize)
        {
            this.batchSize = batchSize;
        }

        public async Task<bool> Forward()
        {
            using var stateHistogram = new StateMetricHistogram(GetIdentifier());

            using var forwardActivity =
                TelemetryTracing.TracingActivitySource.StartActivity(GetIdentifier());

            var messages = GetMessages();

            if (!messages.Any())
            {
                forwardActivity?.SetTag("doneWork", false);
                stateHistogram.SetDoneWork(false);
                return false;
            }

            forwardActivity?.SetTag("polledBatchSize", messages.Count());

            await DoCycle(messages);

            forwardActivity?.SetTag("doneWork", true);
            return true;
        }

        private async Task DoCycle(IEnumerable<IOutboxMessage> messages)
        {
            foreach (var message in messages)
            {
                var messageToDeliver = new Message<string, string> { Value = JsonSerializer.Serialize(message) };

                using var publishActivity =
                    TelemetryTracing.TracingActivitySource.StartActivity("Publish");
                publishActivity?.SetTag("topic", kafkaOptions.OutboxTopic);

                using (PrometheusMetrics.SequentiallyStrategyPublishHistogram.WithLabels(batchSize.ToString()).NewTimer())
                {
                    await producer.ProduceAsync(kafkaOptions.OutboxTopic, messageToDeliver);
                }
            }

            RemoveMessages(messages);

            await context.SaveChangesAsync();
        }

        private void RemoveMessages(IEnumerable<IOutboxMessage> messages)
        {
            context.OutboxMessages.RemoveRange(messages as List<OutboxMessage>);
        }

        private IEnumerable<IOutboxMessage> GetMessages()
        {
            var outboxMessages = context.OutboxMessages
                .Take(batchSize)
                .OrderBy(m => m.CreatedDateTime)
                .ToList();

            using var castActivity = TelemetryTracing.TracingActivitySource.StartActivity("Cast list to interface");

            var messages = outboxMessages
                    .Cast<IOutboxMessage>();
            return messages;
        }

        private string GetIdentifier()
        {
            return $"{ServiceName}_{batchSize}";
        }
    }
}
