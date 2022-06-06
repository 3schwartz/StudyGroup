using grpc;
using grpc.Services;
using Models;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;

services.AddLogging();

services.AddCors(options => options.AddDefaultPolicy(
    builder => builder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader()
    ));

services.AddSingleton<TopicChannels>();
services.AddSingleton<IDroidRepository, DroidRepository>();
services.AddGrpc();

var app = builder.Build();

app.UseRouting();

app.UseCors();

app.UseGrpcWeb();

app.MapGrpcService<DroidService>().EnableGrpcWeb();

app.Run();
