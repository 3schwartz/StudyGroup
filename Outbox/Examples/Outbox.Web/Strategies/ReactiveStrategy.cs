using System.Collections.Immutable;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Outbox.Web.Configurations;
using Outbox.Web.Kafka;
using Outbox.Web.Metrics;
using Outbox.Web.Models;
using Outbox.Web.Telemetry;

namespace Outbox.Web.Strategies
{
    public class ReactiveStrategy : IOutboxStrategy
    {
        private const string ServiceName = "react_strategy";

        private readonly IProducer<string, string> producer;
        private readonly KafkaOptions kafkaOptions;
        private readonly DbContextOptions<OutboxContext> dbOptions;
        private ImmutableHashSet<Guid> messagesInProcess;
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly Func<OutboxMessageReact, bool> inProcess;

        private int sleepWhenNoWork = 1000;
        private int iterationCount = 0;
        private int iterationMax = -1;

        public ReactiveStrategy(ISingletonProducer singletonProducer, IOptions<KafkaOptions> kafkaOptions,
            DbContextOptions<OutboxContext> dbOptions)
        {
            this.dbOptions = dbOptions;
            cancellationTokenSource = new CancellationTokenSource();
            messagesInProcess = ImmutableHashSet<Guid>.Empty;
            this.kafkaOptions = kafkaOptions.Value;
            producer = singletonProducer.GetSingletonProducer();
            inProcess = react => !messagesInProcess.Contains(react.Id);
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

        private IObservable<IList<OutboxMessageReact>> ConstructReadObservable()
        {
            var observable = Observable.Create<IList<OutboxMessageReact>>(observer =>
            {
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    using var statehistogram = new StateMetricHistogram(ServiceName);
                    using var activity = TelemetryTracing.TracingActivitySource.StartActivity(ServiceName);

                    IList<OutboxMessageReact> outboxMessages;
                    using (var outboxContext = new OutboxContext(dbOptions))
                    {
                        outboxMessages = outboxContext.OutboxMessageReact
                        .Where(inProcess)
                        .OrderBy(m => m.CreatedDateTime)
                        .ToList();
                    }

                    if (outboxMessages.Count == 0)
                    {
                        activity?.SetTag("done.work", false);
                        statehistogram.SetDoneWork(false);

                        Thread.Sleep(1000);
                    }
                    else
                    {
                        AddMessagesToProcessSet(outboxMessages);

                        observer.OnNext(outboxMessages);

                        activity?.SetTag("done.work", true);
                        activity?.SetTag("processed.message.count", outboxMessages.Count);
                    }
                }

                return Disposable.Empty;
            });

            return observable;
        }

        private async Task Publish(IList<OutboxMessageReact> messages)
        {
            using var activity = TelemetryTracing.TracingActivitySource.StartActivity($"{ServiceName}_publish");

            foreach (var outboxMessageReact in messages)
            {
                string serializeObject;
                using (var activitySerialize = TelemetryTracing.TracingActivitySource.StartActivity($"{ServiceName}_publish_serialize"))
                {
                    serializeObject = JsonConvert.SerializeObject(outboxMessageReact);
                }

                using (var activityPublish = TelemetryTracing.TracingActivitySource.StartActivity($"{ServiceName}_publish_publish"))
                {
                    await producer.ProduceAsync(kafkaOptions.OutboxTopic, new Message<string, string> { Value = serializeObject });
                }
            }
        }

        private IObserver<IList<OutboxMessageReact>> ConstructDeleteObserver()
        {
            var observerDelete = Observer.Create<IList<OutboxMessageReact>>(messages =>
            {
                using var activity = TelemetryTracing.TracingActivitySource.StartActivity($"{ServiceName}_delete");
                using var outboxContext = new OutboxContext(dbOptions);

                outboxContext.OutboxMessageReact.RemoveRange(messages);

                outboxContext.SaveChanges();

                RemoveMessagesFromProcessSet(messages);

                iterationCount += messages.Count;
                if (iterationMax != -1 && iterationMax <= iterationCount)
                {
                    cancellationTokenSource.Cancel();
                }
            });

            return observerDelete;
        }

        private void RemoveMessagesFromProcessSet(IList<OutboxMessageReact> messages)
        {
            using var removeActivity = TelemetryTracing.TracingActivitySource.StartActivity("Remove if exists");

            foreach (var outboxMessageReact in messages)
            {
                ImmutableInterlocked.Update(ref messagesInProcess,
                    (set, item) => set.Remove(item), outboxMessageReact.Id);
            }

        }

        private void AddMessagesToProcessSet(IList<OutboxMessageReact> outboxMessages)
        {
            using var activityAdd =
                TelemetryTracing.TracingActivitySource.StartActivity($"{ServiceName}_add");

            foreach (var outboxMessageReact in outboxMessages)
            {
                ImmutableInterlocked.Update(ref messagesInProcess,
                    (set, item) => set.Add(item), outboxMessageReact.Id);
            }
        }

        public Task<bool> Forward()
        {
            var readObservable = ConstructReadObservable();
            var deleteObserver = ConstructDeleteObserver();

            readObservable
                .Select(async messages =>
                {
                    await Publish(messages);
                    return messages;
                })
                .Concat()
                .Subscribe(deleteObserver);

            return Task.FromResult(cancellationTokenSource.IsCancellationRequested);
        }
    }

}
