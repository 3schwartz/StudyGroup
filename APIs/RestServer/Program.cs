using Microsoft.EntityFrameworkCore;
using RestServer.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();

var services = builder.Services;

services
    .AddDbContext<StarWarsContext>(opt =>
    {
        opt.UseInMemoryDatabase("StarWars");
        opt.EnableSensitiveDataLogging();
    });

services.AddControllers();

services
    .AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new() { Title = "RestServer", Version = "v1" });
    });

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

using (var scope = app.Services.CreateScope())
{
    using var context = scope.ServiceProvider.GetRequiredService<StarWarsContext>();

    context.Seed();
}

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "RestServer v1"));

app.UseRouting();

app.MapControllers();

app.Run();
