# Practice Chaos
Following the 10.000 hours rule, then you need this amount of time to be expect in something. However often we first debug errors when they occur – and this is often in production. 
Netflix created the Chaos Monkey which was chaos engineering injected into their system. In this way developers where daily challenges with issues and practice everyday bugs which could happen in production – hence they become good in solving serious issues.
This exercise is to use Simmy and inject some Chaos into a simple .NET API. 

## Application

The API is simple. Start the application and target endpoint, ``http://localhost:5201/chaos/something/1`` (with a parameter of 1 or 2) you follow the flow of
-	Query in memory database
-	Send http request 

Also, in between there are some logging just to simulate some methods. 

Already now there is injected one chaos policy – so not all request will succeed!

## Exercise

### Stage 1

Inject following chaos policies
-	Latency on database call
-	Wrong data format from http client
-	Some exception from logging methods
Make sure the chaos are only injected in development mode.

### Stage 2

Now chaos is injected, so now you need to make resilience. Make sure that even with chaos all request gets a response.

## Discussion

Finally, a discussion is made
-	Where does this bring value
-	Have you experience issues which practice could have help?
-	Does it make sense to add chaos like this – example wrong DTO’s returned from external sources


### Reference
Simmy: https://github.com/Polly-Contrib/Simmy