using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using grpcClient;
using Grpc.Net.Client.Web;
using Grpc.Net.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton(services =>
{
    var client = new HttpClient(new GrpcWebHandler(GrpcWebMode.GrpcWebText, new HttpClientHandler()));
    var channel = GrpcChannel.ForAddress(new Uri("https://localhost:7144"), new GrpcChannelOptions { HttpClient = client });
    return new GrpcService.Shared.DroidService.DroidServiceClient(channel);
});

builder.Services.AddLogging();

await builder.Build().RunAsync();
