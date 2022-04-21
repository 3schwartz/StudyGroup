# Exercises
For OData and REST one can use Swagger UI to execute calls.

For GraphQL one can use Banana Cake Pop.

## Render data
Render data for each of the API's. 
Ex. expand in OData case
```
https://localhost:7028/v1/Droids?&$expand=Episodes
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

# Change update action
Change update action in all API's such that not only PrimaryFunction is changed, but also the name.

For REST and OData patch can be done by using body
```
{
  "primaryFunction": "Something awesome"
}
```

For GraphQL one can use query
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
- https://www.odata.org/getting-started/basic-tutorial/
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