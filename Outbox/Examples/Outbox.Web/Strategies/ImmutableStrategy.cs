using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Outbox.Web.Configurations;
using Outbox.Web.Kafka;
using Outbox.Web.Metrics;
using Outbox.Web.Models;
using Outbox.Web.Records;
using Outbox.Web.Telemetry;

namespace Outbox.Web.Strategies
{
    public class ImmutableStrategy : IOutboxStrategy
    {
        private const string ServiceName = "immutable_strategy";

        private readonly DbContextOptions<OutboxContext> dbOptions;
        private readonly KafkaOptions kafkaOptions;
        private readonly IProducer<string, string> producer;
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly Func<OutboxMessageImmutable, bool> aboveCreated;

        private ImmutableQueue<StrategyRecord> messagesRead;
        private ImmutableQueue<StrategyRecord> messagesPublished;
        private ImmutableDictionary<Guid, DateTime> lastMessagesRead;

        private DateTime latestReadCreatedDateTime = new DateTime(1970, 1, 1).ToUniversalTime();
        private int iterationCount = 0;
        private int iterationMax = -1;
        private int sleepWhenNoWork = 1000;

        public ImmutableStrategy(DbContextOptions<OutboxContext> dbOptions, 
            IOptions<KafkaOptions> kafkaOptions,
            ISingletonProducer singletonProducer)
        {
            this.dbOptions = dbOptions;
            this.kafkaOptions = kafkaOptions.Value;
            producer = singletonProducer.GetSingletonProducer();
            messagesRead = ImmutableQueue<StrategyRecord>.Empty;
            messagesPublished = ImmutableQueue<StrategyRecord>.Empty;
            lastMessagesRead = ImmutableDictionary<Guid, DateTime>.Empty;

            cancellationTokenSource = new CancellationTokenSource();

            Expression<Func<OutboxMessageImmutable, bool>>  aboveCreatedExpression = (messageExpr) 
                => messageExpr.CreatedDateTime.CompareTo(latestReadCreatedDateTime) >= 0 && !lastMessagesRead.ContainsKey(messageExpr.Id);
            aboveCreated = aboveCreatedExpression.Compile();
        }

        /// <summary>
        /// Set sleep each thread does when there isn't anything on the queue.
        /// </summary>
        /// <param name="milliseconds"></param>
        public void SetSleepBetweenWork(int milliseconds)
        {
            this.sleepWhenNoWork = milliseconds;
        }

        /// <summary>
        /// Used to set a max amount of messages process through.
        /// </summary>
        /// <param name="iterationMax"></param>
        public void SetIterationMax(int iterationMax)
        {
            this.iterationMax = iterationMax;
        }
        
        public Task<bool> Forward()
        {
            var readMessages = ReadMessages(cancellationTokenSource);
            var publishMessages = PublishMessages(cancellationTokenSource);
            var deleteMessage = DeleteMessage(cancellationTokenSource);

            Task.WaitAll(readMessages, publishMessages, deleteMessage);

            return Task.FromResult(cancellationTokenSource.IsCancellationRequested);
        }


        public Task ReadMessages(CancellationTokenSource cts)
        {
            var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cts.Token);

            return Task.Run(() =>
            {
                while (!linkedTokenSource.IsCancellationRequested)
                {
                    try
                    {
                        var didWork = ReadMessagesCycle();

                        if (!didWork) Thread.Sleep(sleepWhenNoWork);
                    }
                    catch (Exception)
                    {
                        // Swallow
                    }
                }
            }, linkedTokenSource.Token);
        }

        public Task PublishMessages(CancellationTokenSource cts)
        {
            var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cts.Token);

            return Task.Run(async () =>
            {
                while (!linkedTokenSource.IsCancellationRequested)
                {
                    try
                    {
                        var didWork = await PublishMessageCycle();

                        if (!didWork) Thread.Sleep(sleepWhenNoWork);
                    }
                    catch (Exception)
                    {
                        // Swallow
                    }
                }
            }, linkedTokenSource.Token);
        }


        public Task DeleteMessage(CancellationTokenSource cts)
        {
            var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cts.Token);

            return Task.Run(() =>
            {
                while (!linkedTokenSource.IsCancellationRequested)
                {
                    try
                    {
                        var didWork = DeleteMessagesCycle();

                        if (!didWork) Thread.Sleep(sleepWhenNoWork);
                    }
                    catch (Exception)
                    {
                        // Swallow
                    }
                }
            }, linkedTokenSource.Token);
        }

        private bool ReadMessagesCycle()
        {
            using var context = new OutboxContext(dbOptions);
            using var stateHistogram = new StateMetricHistogram(GetReadIdentifier());
            using var activity = TelemetryTracing.TracingActivitySource.StartActivity(GetReadIdentifier());

            var outboxMessageConcurrents = context.OutboxMessageImmutables
                .Where(aboveCreated)
                .OrderBy(m => m.CreatedDateTime)
                .ToList();

            foreach (var message in outboxMessageConcurrents)
            {
                ImmutableInterlocked.Enqueue(ref messagesRead, new StrategyRecord(message, activity?.Context));
            }

            var doneWork = outboxMessageConcurrents.Any();

            if (doneWork)
            {
                AddLastMessageAsState(outboxMessageConcurrents);
            }
            stateHistogram.SetDoneWork(doneWork);
            activity?.SetTag("done.work", doneWork);
            activity?.SetTag("processed.message.count", outboxMessageConcurrents.Count);

            return doneWork;
        }

        private async Task<bool> PublishMessageCycle()
        {
            using var stateHistogram = new StateMetricHistogram(GetPublishIdentifier());

            var tryDequeue = ImmutableInterlocked.TryDequeue(ref messagesRead, out var message);

            if (tryDequeue)
            {
                using var activity = TelemetryTracing.StartActivityWithParentWhenContextNotNull(GetPublishIdentifier(), message.ActivityContext);
                activity?.SetTag("done.work", tryDequeue);

                var messageToDeliver = new Message<string, string> { Value = JsonSerializer.Serialize(message.Message) };
                await producer.ProduceAsync(kafkaOptions.OutboxTopic, messageToDeliver);

                ImmutableInterlocked.Enqueue(ref messagesPublished, new StrategyRecord(message.Message, activity?.Context));
            }

            stateHistogram.SetDoneWork(tryDequeue);
            return tryDequeue;
        }

        private bool DeleteMessagesCycle()
        {
            using var stateHistogram = new StateMetricHistogram(GetDeleteIdentifier());

            var tryDequeue = ImmutableInterlocked.TryDequeue(ref messagesPublished, out var message);

            if (tryDequeue)
            {
                using var activity = TelemetryTracing.StartActivityWithParentWhenContextNotNull(GetDeleteIdentifier(), message.ActivityContext);
                activity?.SetTag("done.work", tryDequeue);

                using var context = new OutboxContext(dbOptions);

                context.OutboxMessageImmutables.Remove((OutboxMessageImmutable)message.Message);

                context.SaveChanges();

                using var removeActivity = TelemetryTracing.TracingActivitySource.StartActivity("Remove if exists");
                {
                    ImmutableInterlocked.TryRemove(ref lastMessagesRead, message.Message.Id, out _);
                }

                iterationCount++;
                if (iterationMax != -1 && iterationMax <= iterationCount)
                {
                    cancellationTokenSource.Cancel();
                }
            }
            
            stateHistogram.SetDoneWork(tryDequeue);

            return tryDequeue;
        }

        private void AddLastMessageAsState(IList<OutboxMessageImmutable> outboxMessageConcurrents)
        {
            using var activity = TelemetryTracing.TracingActivitySource.StartActivity("Add last message as state");
            var lastMessageReadInCycle = outboxMessageConcurrents[^1];

            latestReadCreatedDateTime = lastMessageReadInCycle.CreatedDateTime;

            ImmutableInterlocked.AddOrUpdate(ref lastMessagesRead, lastMessageReadInCycle.Id,
                lastMessageReadInCycle.CreatedDateTime,
                (_, _) => lastMessageReadInCycle.CreatedDateTime);
        }

        private string GetPublishIdentifier()
        {
            return $"{ServiceName}_publish";
        }

        private string GetDeleteIdentifier()
        {
            return $"{ServiceName}_delete";
        }

        private string GetReadIdentifier()
        {
            return $"{ServiceName}_read";
        }
    }
}