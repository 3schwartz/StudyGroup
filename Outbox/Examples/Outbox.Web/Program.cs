using Microsoft.EntityFrameworkCore;
using Npgsql;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Outbox.Web.Configurations;
using Outbox.Web.Factory;
using Outbox.Web.HostedServices;
using Outbox.Web.Kafka;
using Outbox.Web.Models;
using Outbox.Web.Strategies;
using Outbox.Web.Telemetry;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
var configuration = builder.Configuration;

services.AddOptions<KafkaOptions>(configuration.GetSection("Kafka").Value);

services.AddDbContext<OutboxContext>(opts =>
opts.UseNpgsql(configuration.GetConnectionString("Postgres")));

services.AddLogging(b =>
{
    b.AddConfiguration(configuration.GetSection("Logging"));
    b.AddConsole();
    b.AddDebug();
});

services.AddOpenTelemetryTracing(b =>
{
    b
        .AddSource(TelemetryTracing.ServiceName)
        .SetResourceBuilder(
            ResourceBuilder.CreateDefault()
                .AddService(serviceName: TelemetryTracing.ServiceName, serviceVersion: TelemetryTracing.ServiceVersion))
        .AddHttpClientInstrumentation()
        .AddAspNetCoreInstrumentation()
        .AddNpgsql()
        .AddJaegerExporter();
});

services.AddSingleton<IConsumerFactory, ConsumerFactory>();
services.AddSingleton<ISingletonProducer, SingletonProducer>();

services.AddTransient<SequentiallyStrategy>();
services.AddTransient<ConnectStrategy>();
services.AddTransient<ConcurrentStrategy>();
services.AddTransient<ImmutableStrategy>();
services.AddTransient<ReactiveStrategy>();

services.AddHostedService<SingleRowSequentiallyService>();
//services.AddHostedService<BatchSequentiallyService>();
//services.AddHostedService<ConnectService>();
//services.AddHostedService<ConcurrentService>();
//services.AddHostedService<ImmutableService>();
//services.AddHostedService<ReactService>();

var app = builder.Build();

app.UseRouting();

app.MapGet("/", () => "Hello World!");
app.UseEndpoints(endpoints =>
{
    endpoints.MapMetrics();
});

app.Run();