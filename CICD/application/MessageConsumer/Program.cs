using Confluent.Kafka;

var bootstrapServer = Environment.GetEnvironmentVariable("BOOTSTRAP_SERVER") ?? "localhost:9094";
var sleep = Environment.GetEnvironmentVariable("SLEEP_MS") != null && int.TryParse(Environment.GetEnvironmentVariable("SLEEP_MS"), out var sleepTime) ? sleepTime : 10000;
var topic = Environment.GetEnvironmentVariable("TOPIC") ?? "apps";
var groupId = Environment.GetEnvironmentVariable("GROUP") ?? "apps";

var tokenSource = new CancellationTokenSource();

using var consumer = new ConsumerBuilder<string, string>(new ConsumerConfig
{
    GroupId = groupId,
    BootstrapServers = bootstrapServer,
    AutoOffsetReset = AutoOffsetReset.Earliest
}).Build();

consumer.Subscribe(topic);

while (!tokenSource.IsCancellationRequested)
{
    try
    {
        var message = consumer.Consume(tokenSource.Token);

        Console.WriteLine($"Consumed: {message.Message.Value}");
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        await Task.Delay(sleep);
    }

}