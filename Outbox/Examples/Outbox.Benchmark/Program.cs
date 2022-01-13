using System.Linq.Expressions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Outbox.Initializer;
using Outbox.Web.Configurations;
using Outbox.Web.Factory;
using Outbox.Web.Kafka;
using Outbox.Web.Models;
using Outbox.Web.Strategies;

namespace Outbox.Benchmark
{

    public class Program
    {
        [HtmlExporter]
        [MemoryDiagnoser]
        public class OutboxBenchmark
        {
            private readonly int seedSize = 500;
            private readonly string connectionsString = "Host=localhost;Port=5432;Database=outbox;Username=postgres;Password=postgres;Pooling=true;";
            private readonly IOptions<KafkaOptions> kafkaOptions = Options.Create(new KafkaOptions
            {
                OutboxTopic = "benchmark"
            });

            public ISingletonProducer SingletonProducer { get; set; }

            [GlobalSetup]
            public void GlobalSetup()
            {
                SingletonProducer = new SingletonProducer(kafkaOptions);

                Expression<Action<OutboxContext, string>> addMessageSingleRowExpr = (contextAction, message)
                    => contextAction.OutboxMessages.Add(new OutboxMessage { Message = message });
                addMessageSingleRow = addMessageSingleRowExpr.Compile();

                Expression<Action<OutboxContext, string>> addMessageBatchExpr = (contextAction, message)
                    => contextAction.OutboxMessageBatches.Add(new OutboxMessageBatch { Message = message });

                addMessageBatch = addMessageBatchExpr.Compile();
            }

            [GlobalCleanup]
            public void GlobalCleanupAsync()
            {
                SingletonProducer.Dispose();
            }

            #region SingleRow

            public OutboxContext context { get; set; }

            private Action<OutboxContext, string> addMessageSingleRow;

            [IterationSetup(Target = nameof(SingleRow))]
            public void SetupSingleRow()
            {
                SeedDatabase.SeedDatabaseAction(seedSize, addMessageSingleRow);

                var contextOptions = new DbContextOptionsBuilder<OutboxContext>()
                .UseNpgsql(connectionsString)
                .Options;

                context = new OutboxContext(contextOptions);
            }

            [IterationCleanup(Target = nameof(SingleRow))]
            public void CleanupSingleRpw()
            {
                context.Dispose();
            }

            [Benchmark]
            public async Task SingleRow()
            {
                var singleRowStrategy = new SequentiallyStrategy(context, kafkaOptions, SingletonProducer);
                while (await singleRowStrategy.Forward())
                { }
            }

            #endregion

            #region Batch

            private static int batchSize = 50;
            private Action<OutboxContext, string> addMessageBatch;
            public OutboxContext contextBatch { get; set; }

            [IterationSetup(Target = nameof(Batch))]
            public void SetupBatch()
            {
                SeedDatabase.SeedDatabaseAction(seedSize, addMessageBatch);

                var contextOptions = new DbContextOptionsBuilder<OutboxContext>()
                .UseNpgsql(connectionsString)
                .Options;

                contextBatch = new OutboxContext(contextOptions);
            }

            [IterationCleanup(Target = nameof(Batch))]
            public void CleanupBatch()
            {
                contextBatch.Dispose();
            }

            [Benchmark]
            public async Task Batch()
            {
                var batchStrategy = new SequentiallyStrategy(contextBatch, kafkaOptions, SingletonProducer);
                batchStrategy.SetBatchSize(batchSize);
                while (await batchStrategy.Forward())
                {
                }
            }

            #endregion

            #region Connect

            private IConsumerFactory consumerFactory;
            private ISingletonProducer singletonProducerConnect;

            [GlobalSetup(Target = nameof(Connect))]
            public void GlobalSetupConnect()
            {
                Expression<Action<OutboxContext, string>> addMessageConnectExpr = (contextAction, message)
                    => contextAction.OutboxMessageConnect.Add(new OutboxMessageConnect { Message = message });

                SeedDatabase.SeedDatabaseAction(seedSize, addMessageConnectExpr.Compile());

                singletonProducerConnect = new SingletonProducer(kafkaOptions);

                var config = new ConsumerConfig
                {
                    GroupId = kafkaOptions.Value.GroupId,
                    BootstrapServers = kafkaOptions.Value.BootstrapServer,
                    AutoOffsetReset = AutoOffsetReset.Earliest
                };

                consumerFactory = new ConsumerFactory();
                Consumer = consumerFactory.GetConsumer(config, kafkaOptions.Value.ConnectTopic);
            }

            [GlobalCleanup(Target = nameof(Connect))]
            public void GlobalCleanupConnectAsync()
            {
                Consumer?.Close();

                var options = new DbContextOptionsBuilder<OutboxContext>()
                    .UseNpgsql(connectionsString)
                    .Options;

                using var outboxContext = new OutboxContext(options);

                var messages = outboxContext.OutboxMessageConnect.ToList();

                outboxContext.OutboxMessageConnect.RemoveRange(messages);

                outboxContext.SaveChanges();

                singletonProducerConnect.Dispose();
            }

            public IConsumer<string, string> Consumer { get; set; }

            [IterationCleanup(Target = nameof(Connect))]
            public void CleanUpConnect()
            {
                Consumer.Seek(new TopicPartitionOffset(
                    kafkaOptions.Value.ConnectTopic,
                    new Partition(0), Offset.Beginning));
                Thread.Sleep(500);
            }

            [Benchmark]
            public async Task Connect()
            {
                var connectStrategy = new ConnectStrategy(kafkaOptions, consumerFactory, singletonProducerConnect);
                connectStrategy.SetIterationMax(seedSize);
                connectStrategy.Consumer = Consumer;
                await connectStrategy.Forward();
            }

            #endregion

            #region Concurrent

            private Action<OutboxContext, string> addMessageConcurrent;
            private DbContextOptions<OutboxContext> contextConcurrentOptions;
            private ISingletonProducer singletonProducerConcurrent;

            [GlobalSetup(Target = nameof(Concurrent))]
            public void SetupGlobalConcurrent()
            {
                Expression<Action<OutboxContext, string>> addExpr = (contextAction, message)
                    => contextAction.OutboxMessageConcurrent.Add(new OutboxMessageConcurrent { Message = message });

                addMessageConcurrent = addExpr.Compile();

                singletonProducerConcurrent = new SingletonProducer(kafkaOptions);
            }

            [GlobalCleanup(Target = nameof(Concurrent))]
            public void GlobalCleanupConcurrent()
            {
                singletonProducerConcurrent.Dispose();
            }

            [IterationSetup(Target = nameof(Concurrent))]
            public void SetupConcurrent()
            {
                SeedDatabase.SeedDatabaseAction(seedSize, addMessageConcurrent);

                contextConcurrentOptions = new DbContextOptionsBuilder<OutboxContext>()
                    .UseNpgsql(connectionsString)
                    .Options;
            }

            [Benchmark]
            public async Task Concurrent()
            {
                var concurrentStrategy = new ConcurrentStrategy(contextConcurrentOptions, kafkaOptions, singletonProducerConcurrent);
                concurrentStrategy.SetIterationMax(seedSize);
                concurrentStrategy.SetSleepBetweenWork(100);
                await concurrentStrategy.Forward();
            }
            #endregion

            #region React

            private Action<OutboxContext, string> addMessageReactive;
            private DbContextOptions<OutboxContext> contexReactiveOptions;
            private ISingletonProducer singletonProducerReactive;

            [GlobalSetup(Target = nameof(Reactive))]
            public void SetupGlobalReact()
            {
                Expression<Action<OutboxContext, string>> addExpr = (contextAction, message)
                    => contextAction.OutboxMessageReact.Add(new OutboxMessageReact { Message = message });

                addMessageReactive = addExpr.Compile();

                singletonProducerReactive = new SingletonProducer(kafkaOptions);
            }

            [GlobalCleanup(Target = nameof(Reactive))]
            public void GlobalCleanupReact()
            {
                singletonProducerReactive.Dispose();
            }

            [IterationSetup(Target = nameof(Reactive))]
            public void SetupReact()
            {
                SeedDatabase.SeedDatabaseAction(seedSize, addMessageReactive);

                contexReactiveOptions = new DbContextOptionsBuilder<OutboxContext>()
                    .UseNpgsql(connectionsString)
                    .Options;
            }

            [Benchmark]
            public async Task Reactive()
            {
                var reactiveStrategy = new ReactiveStrategy(singletonProducerReactive, kafkaOptions, contexReactiveOptions);
                reactiveStrategy.SetIterationMax(seedSize);
                reactiveStrategy.SetSleepBetweenWork(100);
                await reactiveStrategy.Forward();
            }
            #endregion

            #region Immutable

            private Action<OutboxContext, string> addMessageImmutable;
            private DbContextOptions<OutboxContext> contextImmutableOptions;
            private ISingletonProducer singletonProducerImmutable;

            [GlobalSetup(Target = nameof(Immutable))]
            public void SetupGlobalImmutable()
            {
                Expression<Action<OutboxContext, string>> addExpr = (contextAction, message)
                    => contextAction.OutboxMessageImmutables.Add(new OutboxMessageImmutable { Message = message });

                addMessageImmutable = addExpr.Compile();

                singletonProducerImmutable = new SingletonProducer(kafkaOptions);
            }

            [GlobalCleanup(Target = nameof(Immutable))]
            public void GlobalCleanupImmutable()
            {
                singletonProducerImmutable.Dispose();
            }

            [IterationSetup(Target = nameof(Immutable))]
            public void SetupImmutable()
            {
                SeedDatabase.SeedDatabaseAction(seedSize, addMessageImmutable);

                contextImmutableOptions = new DbContextOptionsBuilder<OutboxContext>()
                    .UseNpgsql(connectionsString)
                    .Options;
            }

            [Benchmark]
            public async Task Immutable()
            {
                var immutableStrategy = new ImmutableStrategy(contextImmutableOptions, kafkaOptions, singletonProducerImmutable);
                immutableStrategy.SetIterationMax(seedSize);
                immutableStrategy.SetSleepBetweenWork(100);
                await immutableStrategy.Forward();
            }
            #endregion

        }

        static void Main(string[] args)
        {
            BenchmarkRunner.Run<OutboxBenchmark>(DefaultConfig.Instance, args);
        }
    }
}