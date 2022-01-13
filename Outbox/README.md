# Introduction
Try and beat the simplest design of the Outbox pattern! To do this you would need to think asynchronously and thinking about how you can make observable code.

All the way back in 2011, Martin Fowler did an interesting article named The [LMAX Architecture](https://martinfowler.com/articles/lmax.html). This article describes how a financial institution was able to handle over 6 million orders pr second within a single thread.

In short what they did was abstracting all their asynchronously logic in separate flows and first when all events has been done, they called the main busines logic.

He actually writes about how this isn’t what we today will call classic async microservice architecture with messages bus between applications since this design involves a lot of I/O actions whenever sending or receiving – instead all of this was keep in memory.

# Exercise
Have these kinds of thinking in your head when trying to develop the Outbox pattern. In simple for the Outbox pattern is solving the need when one want to publish the same messages to several storages and ensuring at least once delivery. An example could be so ensuring both writing to a database and a message queue.

The outbox pattern solves this by storing the messages to the database and yet to another table. A separate thread will then read from this table, send to any message queue and then either mark or delete the row from the table. By this one ensures that data has both been delivered to the database and the message bus.

The exercise is to develop as code with does these actions as fast as possible. You should benchmark it against the default design described below.

When designing fast code, you quickly find yourself asking – how is my code performing? Turn to two of the pillars of observability which is tracing, and metrics.

You can use Prometheus to wrap any junk of code within timers, and you can use Jaeger to wrap code blocks within activities such that you would know exactly what your trace is spending time on. Regarding how to use them read below.

Happy coding 😊

## Benchmark
One benchmark which is the simplest form of Outbox pattern one can create. In this design a row as read from the database, published to a message queue, and then deleted from the database.

If we think in asynchronously actions one can say that reading from the database is one – this could be isolated. 

Likewise, the sending to the message bus and deleting from the database.

This is already given in the solution and in the project `Outbox.Benchmark`. This is were you can put your code when benchmarking against the simple one.

# Getting started
Within the main folder a `docker-compose.yml` file is given. This will start up
-	Postgres
-	Pgadmin
-	Kafka
-	Zookeeper
-	Kowl
-	Jaeger
-	Prometheus

To get it up and running use 
```
docker-compose up
```
When started from within the Outbox.Web run
```
dotnet ef database update
```
This will apply all migrations.


# Observability
## Prometheus
Access at http://localhost:9000.
Examples can be found at https://github.com/prometheus-net/prometheus-net.
## Jaeger
Access at http://localhost:9000.
Example of how to do a tracing block
```
using (var activitySerialize = TelemetryTracing.TracingActivitySource.StartActivit("SomeUnique"))
{
    /// some code one wants to trace.
}
```
Any trace within this block will automatically get linked to its parent. To explicit link a parent – example between queues, use
```
using (var activity = TelemetryTracing.StartActivityWithParentWhenContextNotNull("SomeUnique", parentAcitivyContext)){
	/// some code which one wants to trace
}
```
# Seed
A project named `Outbox.Initialize` is given. This is a initializer which you can run when testing your design and code.

# Database
Access at http://localhost:8080.
Host, username and password are `postgres`.

# Examples
Within the subfolder `Examples` you can find some examples where different design are benchmarked. I encourage you to try yourself first 😊


# Note
All these tests are done on local environment and within one network. In the real-world networks latencies and batching would have MUCH more effect. The exercise is however fun and one get’s some good ideas how one can performance test written code, and how one can use observability to write better code.
