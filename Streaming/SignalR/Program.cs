using Models;
using SignalRShared;
using SignalRShared.Hubs;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;

services.AddLogging();

services.AddCors(options => options.AddDefaultPolicy(
    builder => builder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader()
    ));

services.AddSignalR();

services.AddSingleton<IDroidRepository, DroidRepository>();
services.AddSingleton<Subscription>();

var app = builder.Build();

app.UseCors();

app.MapHub<DroidHub>("/droidHub");

app.Run();
