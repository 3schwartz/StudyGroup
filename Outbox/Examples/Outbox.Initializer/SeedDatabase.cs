using Microsoft.EntityFrameworkCore;
using Outbox.Web.Models;

namespace Outbox.Initializer
{
    public static class SeedDatabase
    {
        private static readonly string connectionsString = "Host=localhost;Port=5432;Database=outbox;Username=postgres;Password=postgres;Pooling=true;";

        public static void Seed(int numberOfRows)
        {
            var contextOptions = new DbContextOptionsBuilder<OutboxContext>()
            .UseNpgsql(connectionsString)
            //.LogTo(Console.WriteLine)
            //.EnableSensitiveDataLogging()
            .Options;

            using var context = new OutboxContext(contextOptions);

            for (int i = 0; i < numberOfRows; i++)
            {
                try
                {
                    var message = RandomMessageGenerator.RandomString(8);
                    //context.OutboxMessages.Add(new OutboxMessage { Message = message });
                    //context.OutboxMessageBatches.Add(new OutboxMessageBatch { Message = message });
                    //context.OutboxMessageConnect.Add(new OutboxMessageConnect { Message = message });
                    //context.OutboxMessageConcurrent.Add(new OutboxMessageConcurrent { Message = message });
                    //context.OutboxMessageImmutables.Add(new OutboxMessageImmutable { Message = message });
                    context.OutboxMessageReact.Add(new OutboxMessageReact { Message = message });

                    context.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                if (i % 5 == 0)
                { 
                    Thread.Sleep(1000);
                }
            }
        }
        
        public static void SeedDatabaseAction(int numberOfRows, Action<OutboxContext, string> databaseAdd)
        {
            var contextOptions = new DbContextOptionsBuilder<OutboxContext>()
                .UseNpgsql(connectionsString)
                .Options;

            using var context = new OutboxContext(contextOptions);

            for (int i = 0; i < numberOfRows; i++)
            {
                var message = RandomMessageGenerator.RandomString(8);

                databaseAdd(context, message);
                context.SaveChanges();
            }
        }
    }
}