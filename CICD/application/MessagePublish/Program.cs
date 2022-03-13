using Confluent.Kafka;
using System.Diagnostics;

var bootstrapServer = Environment.GetEnvironmentVariable("BOOTSTRAP_SERVER") ?? "localhost:9094";
var sleep = Environment.GetEnvironmentVariable("SLEEP_MS") != null && int.TryParse(Environment.GetEnvironmentVariable("SLEEP_MS"), out var sleepTime) ? sleepTime : 10000;
var endpoint = Environment.GetEnvironmentVariable("ENDPOINT") ?? "http://localhost:5271";
var topic = Environment.GetEnvironmentVariable("TOPIC") ?? "apps";

var tokenSource = new CancellationTokenSource();

using var client = new HttpClient();
using var producer = new ProducerBuilder<string, string>(new ProducerConfig
{
    BootstrapServers = bootstrapServer,
})
    .Build();

var now = DateTime.UtcNow;
var stopwatch = Stopwatch.StartNew();

while (!tokenSource.IsCancellationRequested)
{


    try
    {
        var response = await client.GetAsync($"{endpoint}/version");
        var version = await response.Content.ReadAsStringAsync();

        var currentTime = now.AddMilliseconds(stopwatch.ElapsedMilliseconds);

        var message = new Message<string, string>
        {
            Value = $"{currentTime.ToString("O")}: version: {version}"
        };

        await producer.ProduceAsync(topic, message);

        Console.WriteLine($"{currentTime.ToString("O")}: send message with version: {version}");
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }
    finally
    {
        await Task.Delay(sleep);
    }

}