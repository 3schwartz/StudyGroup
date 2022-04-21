using Common.Models;
using GraphQLServer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;

services
    .AddDbContext<StarWarsContext>(o => o.UseInMemoryDatabase("GraphQL"));

services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    using var context = scope.ServiceProvider.GetRequiredService<StarWarsContext>();

    context.Seed();
}

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapGraphQL();
});

app.Run();
