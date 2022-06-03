using Benchmark;
using Benchmark.Hubs;
using GrpcService.Bechmark;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;

services.AddCors(options => options.AddDefaultPolicy(
    builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

services.AddGraphQLServer()
    .AddQueryType<Query>();

services.AddGrpc();

services.AddSignalR();

services.AddControllers();

services.AddHostedService<BenchmarkBackgroundService>();

var app = builder.Build();

app.UseCors();

app.MapGraphQL();

app.UseGrpcWeb();

app.MapHub<DroidBenchmarkHub>("/droidHub");

app.MapGrpcService<GrpcBenchmarkService>().EnableGrpcWeb();

app.MapGet("/minimal", () => new Droid { Name = "R5-D4", PrimaryFunction = "Astromech" });

app.MapControllers();

app.Run();
