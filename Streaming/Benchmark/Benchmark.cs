using Benchmark.Hubs;
using BenchmarkDotNet.Attributes;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Microsoft.AspNetCore.SignalR.Client;
using Models;

namespace Benchmark
{
    [MemoryDiagnoser]
    [SkewnessColumn, KurtosisColumn]
    [MinIterationCount(20)]
    [SimpleJob(launchCount: 2, warmupCount:2)]
    public class Benchmark
    {
        private IBenchmarkClient? graphqlClient;
        private GrpcService.Bechmark.BechmarkService.BechmarkServiceClient? grpcClient;
        private HubConnection? hubConnection;
        private HttpClient? minimalClient;
        private HttpClient? controllerClient;

        [GlobalSetup]
        public async Task GlobalSetup()
        {
            #region GraphQL
            var services = new ServiceCollection();
            services
                .AddBenchmarkClient()
                .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://localhost:7076/graphql/"));

            var provider = services.BuildServiceProvider();

            graphqlClient = provider.GetRequiredService<IBenchmarkClient>();
            #endregion

            #region gRPC
            var servicesGrpc = new ServiceCollection();
            servicesGrpc.AddSingleton(services =>
            {
                var client = new HttpClient(new GrpcWebHandler(GrpcWebMode.GrpcWebText, new HttpClientHandler()));
                var channel = GrpcChannel.ForAddress(new Uri("http://localhost:9431"), new GrpcChannelOptions { HttpClient = client });
                return new GrpcService.Bechmark.BechmarkService.BechmarkServiceClient(channel);
            });
            var providerGrpc = servicesGrpc.BuildServiceProvider();

            grpcClient = providerGrpc.GetRequiredService<GrpcService.Bechmark.BechmarkService.BechmarkServiceClient>();
            #endregion

            #region SignalR
            hubConnection = new HubConnectionBuilder()
                .WithUrl("https://localhost:7076/droidHub")
                .Build();
            
            await hubConnection.StartAsync();
            #endregion

            #region MinimalAPI
            minimalClient = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7076")
            };
            #endregion
            
            #region ControllerAPI
            controllerClient = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7076")
            };
            #endregion
        }

        [Benchmark(Baseline = true)]
        public async Task ControllerAPI()
        {
            var response = await controllerClient!.GetAsync("controller");
            _ = await response.Content.ReadFromJsonAsync<Droid>();
        }

        [Benchmark]
        public async Task MinimalAPI()
        {
            var response = await minimalClient!.GetAsync("minimal");
            _ = await response.Content.ReadFromJsonAsync<Droid>();
        }

        [Benchmark]
        public async Task GraphQL()
        {
            _ = await graphqlClient!.DroidBenchmark.ExecuteAsync();
        }

        [Benchmark]
        public async Task Grpc()
        {
            _ = await grpcClient!.DroidBenchmarkAsync(new Google.Protobuf.WellKnownTypes.Empty());
        }

        [Benchmark]
        public async Task SignalR()
        {
            _ = await hubConnection!.InvokeAsync<Droid>(DroidBenchmarkHub.DroidBenchmarkMethod);
        }
    }
}
