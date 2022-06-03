# Study Group

This repository contains projects and exercises which I have made for study groups sessions I have hosted. 

Topics for sessions hosted are described in below sections and exercises can be found in their respective folders.

## Memory Allocation

Investigating memory allocation, stack vs. heap, service lifetime and memory leaks.

## Outbox pattern

Investigating Outbox pattern hence how one can ensure delivery of messages to both a database and message bus.

Examples of different designs with pull, fetch reactive pattern and [Kafka Connect](https://kafka.apache.org/documentation/#connectconfigs) is given and tested using [BenchmarkDotNet](https://benchmarkdotnet.org/articles/overview.html).

Observability is also discussed how one can use [tracing](https://opentelemetry.io/) and [Jaeger](https://www.jaegertracing.io/) to observe message flow.

## Chaos
Investigation of [Chaos engineering](https://en.wikipedia.org/wiki/Chaos_engineering) and how one can use [Simmy](https://github.com/Polly-Contrib/Simmy) to simulate chaos in our applications.

## CICD
Investigation of CI pipelines like [Azure pipelines](https://azure.microsoft.com/en-us/services/devops/pipelines/) and [Github Actions](https://github.com/features/actions) and exercises around building these.

Covering CD with the perspective of using the fantastic tool from [Argo CD](https://argo-cd.readthedocs.io/en/stable/) in a Kubernetes cluster.

## APIs

Covering three different backends APIs developed using [REST](https://dotnet.microsoft.com/en-us/apps/aspnet/apis), [OData](https://www.odata.org/) and [GraphQL](https://graphql.org/).

Discussion of versioning, changes needed in client at changes from backend, batching functionality, and general pros- and cons.

## Streaming

Three different frameworks which can be used for streaming are covered. [GraphQL](https://graphql.org/), [gRPC](https://grpc.io/) and [SignalR](https://dotnet.microsoft.com/en-us/apps/aspnet/signalr).

When discussion these frameworks some interesting topics are the community contribution, schema alignment between server and clients and of course performance.