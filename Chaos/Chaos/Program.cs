using Chaos.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();

var services = builder.Services;

services.AddHttpClient();

services.AddDbContext<ChaosContext>(opts =>
    opts.UseInMemoryDatabase("Chaos"));

services.AddControllers();

services.AddHostedService<Seed>();

var app = builder.Build();

app.UseRouting();

app.MapControllers();

app.Run();
