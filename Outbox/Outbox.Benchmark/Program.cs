using System.Linq.Expressions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Outbox.Initializer;
using Outbox.Web.Configurations;
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

        }

        static void Main(string[] args)
        {
            BenchmarkRunner.Run<OutboxBenchmark>(DefaultConfig.Instance, args);
        }
    }
}