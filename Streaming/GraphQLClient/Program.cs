using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using GraphQLClient;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services
    .AddDroidClient()
    .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://localhost:7149/graphql/"))
    .ConfigureWebSocketClient(client => client.Uri = new Uri("wss://localhost:7149/graphql/"));

await builder.Build().RunAsync();
