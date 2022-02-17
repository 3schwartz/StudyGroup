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
                    context.OutboxMessages.Add(new OutboxMessage { Message = message });
                    //context.outboxmessagebatches.add(new outboxmessagebatch { message = message });
                    //context.outboxmessageconnect.add(new outboxmessageconnect { message = message });
                    //context.outboxmessageconcurrent.add(new outboxmessageconcurrent { message = message });
                    //context.outboxmessageimmutables.add(new outboxmessageimmutable { message = message });
                    //context.outboxmessagereact.add(new outboxmessagereact { message = message });

                    context.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                if (i % 5 == 0)
                {
                    Console.WriteLine(i);
                    Thread.Sleep(300);
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