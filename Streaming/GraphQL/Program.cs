using GraphQL;
using Models;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;

services.AddSingleton<IDroidRepository, DroidRepository>();

services.AddCors(options => options.AddDefaultPolicy(
    builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

services.AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddSubscriptionType<Subscription>()
    .AddInMemorySubscriptions();

var app = builder.Build();

app.UseCors();

app.UseWebSockets();

app.MapGraphQL();

app.Run();
