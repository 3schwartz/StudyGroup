# Streaming

GraphQL, SignalR and gRPC will be covered. For each of the framework both a server and client is given in the examples. This workshop will only cover the server parts, and the clients are given to be able to play around interactively and simulate server and UI use cases.

Each UI consist of three blocks
- Rendering objects
  In this workshop one is just rendering current state of Droids at start up.
- Updating backend state
  In this workshop one is adding a Droid.
- Stream from backend to frontend
  On every update made, the stream will publish the update.

The streaming block is what we will have focus on. The other two are just given to interactively trigger updates from the stream.

## MVP's

### GraphQL

To enable streaming from GraphQL one needs to add below blocks at start up

````
services.AddGraphQLServer()
    .AddSubscriptionType<Subscription>()
    .AddInMemorySubscriptions();

...
app.UseWebSockets();
app.MapGraphQL();
````

By this one have enabled usages of websocket and added internal in memory messaging system. The `Subscription` class has a implementation of a simple subscription.

```
public class Subscription
{
    [Subscribe]
    [Topic]
    public Task<DroidCreated> OnDroidAddedAsync(
        [EventMessage]Droid droid)
    {
        return Task.FromResult(new DroidCreated(droid, DateTime.UtcNow));
    }
    public record DroidCreated(Droid Droid, DateTime Timestamp);
}
```

`[Subscribe]` tells GraphQL to map this as a subscription. `[Topic]` is used for the internal messaging system to partition messages on topics. Hence messages published to `OnDroidAddedAsync` will be send to subscribes of above subscription.

To trigger the event one can call the above method as below - in below example events are send on every time a droid is added:
```
public async Task<DroidPayload> AddDroidAsync(
    AddDroidInput input,
    [Service]IDroidRepository droids,
    [Service]ITopicEventSender eventSender)
{
    Droid droid = droids.Add(input.Name, input.PrimaryFunction);
    await eventSender.SendAsync(nameof(Subscription.OnDroidAddedAsync), droid);
    return new DroidPayload(droid);
}
```

To see this in action, start the GraphQL project and from Banana Cake Pop *(UI rendered at start up)* execute
```subscription {
  onDroidAdded {
    droid {
      name
      primaryFunction
    }
    timestamp
  }
}
```

Initially nothing will happen. Now in another window execute
```
mutation {
  addDroid(input: { name: "R5-D4", primaryFunction: "Astromech" }) {
    droid {
      name
      primaryFunction
    }
  }
}
```

Go back to the subscription - you should now see a event has been received as below.
```
{
  "data": {
    "onDroidAdded": {
      "droid": {
        "name": "R5-D4",
        "primaryFunction": "Astromech"
      },
      "timestamp": "2022-05-28T15:59:48.975Z"
    }
  }
}
```


```
subscription {
  onMinimal {
    timestamp
    message
  }
}
```


### GRPC

To enable Grpc one needs to add below blocks at start up

````
services.AddGrpc();
...
app.MapGrpcService<GrpcService>();
````

Also, one needs to add a `.proto` file to the project - this is the schema. A minimal example below would be

```
syntax = "proto3";

option csharp_namespace = "GrpcService.Minimal";

import "google/protobuf/empty.proto";

package benchmark;

service GrpcService{
	rpc DoMinimal(google.protobuf.Empty) returns (Minimal);
}

message Minimal {
	string name = 1;
}
```
I'm defining one call, `DoMinimal`, from my service `GrpcService`. This doesn't require any input and returns a `Minimal` object.

Adding nuget package `Grpc.AspNetCore`, add below to `.csproj` and afterwards build the project. Then all services and classes are generated.
```
<ItemGroup>
	<Protobuf Include="Protos\minimal.proto"/>
</ItemGroup>
```

The last part is to build a simple service, which overrides the call given in the `proto`-file.
```
internal class GrpcService : MinimalService.MinimalServiceBase
{
    public override Task<Droid> DoMinimal(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new Minimal { Name = "Something");
    }
}
```

To validate everything works I use [bloomrpc](https://github.com/bloomrpc/bloomrpc). By adding the `proto`-file and invoking without any body, I'm getting response
```
{
  "minimal": "Something"
}
```


### SignalR


## Exercises

For each of the frameworks add a new subscription/stream to both the server and client applications. Follow below steps

- Add a HostedService which every X second publish a new message
- Add necessary updates to schemas. 
    
    When updating GraphQL one needs relevant CLI tools, see https://chillicream.com/docs/strawberryshake/get-started/console#step-1-add-the-strawberry-shake-cli-tools.
    
    After downloading the tools make sure the server is running and then run `dotnet graphql update.`
- Create the server code
- Create the client code
- Launch both the server and client and validate messages are streamed.


## Benchmark

It is always fun to Benchmark - therefore I have created a project `Benchmark`, where five API has been tested

- Controllers
- Minimal API
- GraphQL
- gRPC
- SignalR

I have not tested streaming but just a simple call with a object returned. The Benchmark is running as a background service and calling endpoints on same server. The RTT is hence negligible. There are also disturbance running test like this, however it is fun and gives some ideas how one can performance test APIs.

Results are given below.

``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19043.1706 (21H1/May2021Update)
Intel Core i7-8565U CPU 1.80GHz (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK=6.0.201
  [Host]     : .NET 6.0.3 (6.0.322.12309), X64 RyuJIT
  Job-FFTLKM : .NET 6.0.3 (6.0.322.12309), X64 RyuJIT

LaunchCount=2  MinIterationCount=20  WarmupCount=2  

```
|        Method |     Mean |    Error |    StdDev |   Median | Skewness | Kurtosis | Ratio | RatioSD | Allocated |
|-------------- |---------:|---------:|----------:|---------:|---------:|---------:|------:|--------:|----------:|
| ControllerAPI | 238.0 μs |  7.95 μs |  28.59 μs | 223.0 μs |   0.9780 |    3.207 |  1.00 |    0.00 |      4 KB |
|    MinimalAPI | 260.1 μs |  3.74 μs |   8.60 μs | 257.9 μs |   1.2943 |    4.634 |  1.20 |    0.07 |      4 KB |
|       GraphQL | 435.5 μs | 25.81 μs | 103.20 μs | 397.7 μs |   2.6143 |    9.782 |  1.71 |    0.24 |      8 KB |
|          Grpc | 505.8 μs | 33.91 μs | 106.17 μs | 517.7 μs |  -0.0293 |    2.197 |  2.24 |    0.53 |     11 KB |
|       SignalR | 261.3 μs |  6.19 μs |  19.64 μs | 262.1 μs |   0.1287 |    1.904 |  1.15 |    0.13 |      2 KB |


## References
- https://chillicream.com/docs/strawberryshake/get-started
- https://github.com/ChilliCream/graphql-workshop
- https://docs.microsoft.com/en-us/aspnet/core/blazor/tutorials/signalr-blazor?view=aspnetcore-6.0&tabs=visual-studio&pivots=server
- https://docs.microsoft.com/en-us/aspnet/core/tutorials/signalr?view=aspnetcore-6.0&tabs=visual-studio
- https://docs.microsoft.com/en-us/aspnet/core/signalr/streaming?view=aspnetcore-6.0
- https://blog.jetbrains.com/dotnet/2021/07/19/getting-started-with-asp-net-core-and-grpc/