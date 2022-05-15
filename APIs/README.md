# MVP

## REST

REST API's can be constructed by controllers. Each class which inherits from either `Controller` or `ControllerBase` would be registered as a endpoint.

To register controllers simple inject below into the dependency injection *(DI)* container, and add middleware:

```
services.AddControllers();
...
app.UseRouting();
app.MapControllers();
```

To create a simple endpoint, create a class which inherits from `Controller`. The below will be accessible at path `/hello`.

```
[Route("[controller]")]
public class HelloController : Controller
{
    [HttpGet]
    public IActionResult GetHello()
    {
        return Ok("Hello");
    }
}
```

## OData

OData relies on a **E**ntity **D**ata **M**odel, [*(EDM)*](https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/entity-data-model). One can easily build this from C# classes and hence make a easily combine OData and Entity Framework.

Add Nuget Packages `Microsoft.AspNetCore.OData Version="8.0.9`.

To register OData endpoints (controllers) and EDM model from classes, add below in start up:

```
static IEdmModel GetHelloEdmModel()
{
    var builder = new ODataConventionModelBuilder();
    builder.EntitySet<Hello>("Hello");
    return builder.GetEdmModel();
}
...
services
    .AddControllers()
    .AddOData(opt => opt
        .AddRouteComponents("v1", GetHelloEdmModel()));
...
app.UseRouting();

app.MapControllers();
```
with `Hello.cs` as
```
public class Hello
{
    public int Id { get; set; }
    public string Message { get; set; } = "Hello";
}
```
Like with REST, ASP.NET will register OData endpoints from classes which inherits from `ODataController`.


Build a controller, `HelloController.cs`, as 
```
public class HelloController : ODataController
{
    public IActionResult Get()
    {
        return Ok("Hello");
    }
}
```

Now endpoint `/v1/Hello` can be called.

## GraphQL

Where OData and REST thinks in classic GET, POST, DELETE, PATCH, PUT, GraphQL relies on queries or mutations.

Add Nuget Packages `HotChocolate.AspNetCore Version="12.7.0`.

To create a simple query create class `HelloQuery.cs`:
```
public class HelloQuery
{
    public Hello GetHello()
    {
        return new Hello();
    }
}

public record Hello(string Message = "Hello");

```

In your start up add GraphQL to your DI container and map GraphQL endpoints by adding below:

```
services
    .AddGraphQLServer()
    .AddQueryType<HelloQuery>();

...
app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapGraphQL();
});
```

Start the application and Banana Cake Pop will show up. Now execute below query and you will see the `Hello` object returned.
```
{
  hello {
    message
  }
}
```


# Exercises
For OData and REST one can use Swagger UI to execute calls.

For GraphQL one can use Banana Cake Pop.

## Render data
Render data for each of the API's. 
Ex. expand in OData case
```
https://localhost:7028/v1/Droids?&$expand=Episodes

// or with select
https://localhost:7028/v1/Droids?&$select=Id,Name&$expand=Episodes
```

and GraphQL would be 

```
{
  droids{
    id
    name
    primaryFunction
    episodes {
      id,
      title,
    }
  }
}
```

Now extend Droids to have one additional property and see how many steps are needed to

- Make all API robust for change
- Are the data rendered the same?
- What should be done if new fields should be rendered by same calls to clients

## Change update action
Change update action in all API's such that not only `PrimaryFunction` is changed, but also the `Name`.

REST and OData patch can be done by using body
```
{
  "primaryFunction": "Something awesome"
}
```

GraphQL mutations can be done using query
```
mutation {
  droidPrimaryFunctionChange(input: {
    id: 1
    primaryFunction: "Imperial Inventory"
  }) {
    droid {
      id
      name
      primaryFunction
    }
  }
}
```

# OData

## Versioning

One way would be to specify different EDM models for different paths
```
services
    .AddControllers()
    .AddOData(opt => opt.AddRouteComponents("v1", GetEdmModel())
    .AddRouteComponents("v2", GetEdmModel()));

```

Another way would be using the same Edm model and getting the version as a template forwarded to the API.

In configuration specify 
```
services
    .AddControllers()
    .AddOData(opt => opt.AddRouteComponents("v{version}", GetEdmModel()));
```

And then in controller the template parameters would we injected
```
[HttpGet]
[EnableQuery]
public IActionResult Get(string version)
{
    // do something with version
}
```

### References
- https://www.odata.org/getting-started/basic-tutorial/ (help with queries)
- https://devblogs.microsoft.com/odata/tutorial-creating-a-service-with-odata-8-0/
- https://devblogs.microsoft.com/odata/up-running-w-odata-in-asp-net-6/
- https://dev.to/berviantoleo/odata-with-net-6-5e1p

### Client
- https://docs.microsoft.com/en-us/aspnet/web-api/overview/odata-support-in-aspnet-web-api/odata-v3/calling-an-odata-service-from-a-net-client
- https://docs.microsoft.com/en-us/odata/client/getting-started

# GraphQL

## Queries

### Get a droid by od
```
{
  droidById(id: 1){
    id
    name
    primaryFunction
    episodes {
      title
    }
  }
}
```

### Add a droid

```
mutation addDroids{
  addDroid(input: {
      name: "R5-D4"
      primaryFunction: "Astromech",
      episodes: {
        ids: [1,2]
        }
      }) {
        droid {
          id
          name
          episodes {
            title
          }
        }
      }
}
```

## Versioning
GraphQL avoids versioning:

https://graphql.org/learn/best-practices/#versioning

### Client
- https://chillicream.com/docs/strawberryshake/get-started
- https://chillicream.com/docs/strawberryshake/subscriptions
- https://chillicream.com/blog/2019/11/25/strawberry-shake_2

### References
- https://github.com/ChilliCream/graphql-workshop
- https://graphql.org/code/#c-net
- https://chillicream.com/docs/hotchocolate/get-started