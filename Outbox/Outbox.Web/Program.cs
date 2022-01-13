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

services.AddHostedService<SingleRowSequentiallyService>();

var app = builder.Build();

app.UseRouting();

app.MapGet("/", () => "Hello World!");
app.UseEndpoints(endpoints =>
{
    endpoints.MapMetrics();
});

app.Run();