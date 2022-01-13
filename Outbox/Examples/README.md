# Examples

Following designs of the Outbox pattern has been benchmarked
-	Single row read, publish and delete
-	Batch read, publish and delete
-	Async with concurrent queues
-	Async with immutable queues
-	Kafka Connect
-	Async using Reactive


Results are given in below

``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19043.1415 (21H1/May2021Update)
Intel Core i7-8565U CPU 1.80GHz (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK=6.0.101
  [Host]     : .NET 6.0.1 (6.0.121.56705), X64 RyuJIT
  Job-RIBGKJ : .NET 6.0.1 (6.0.121.56705), X64 RyuJIT

InvocationCount=1  UnrollFactor=1  

```
|     Method |    Mean |    Error |   StdDev |      Gen 0 |     Gen 1 | Allocated |
|----------- |--------:|---------:|---------:|-----------:|----------:|----------:|
|  SingleRow | 7.754 s | 0.0845 s | 0.0706 s |  2000.0000 |         - |     11 MB |
|      Batch | 7.762 s | 0.0166 s | 0.0155 s |  1000.0000 |         - |      4 MB |
|    Connect | 7.615 s | 0.0141 s | 0.0125 s |  1000.0000 |         - |      4 MB |
| Concurrent | 7.891 s | 0.0949 s | 0.0741 s | 11000.0000 | 1000.0000 |     49 MB |
|   Reactive | 8.096 s | 0.0135 s | 0.0127 s |  2000.0000 |         - |     11 MB |
|  Immutable | 7.876 s | 0.0498 s | 0.0466 s | 12000.0000 | 1000.0000 |     50 MB |


## Getting started
From the main folder run
```
docker-compose up
```
When started from within the `Outbox.Web` dir run
```
dotnet ef database update
```
This will apply all migrations.

To get Kafka Connect up and running go to the KafkaConnect dir and run
```
docker-compose up
```
This will setup a source connector against the outbox table and forward any message written to this table to Kafka.
