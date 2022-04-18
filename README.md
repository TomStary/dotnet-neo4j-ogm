# Neo4j OGM for .NET

| :bangbang: | This is proof of concept |
| :--------: | :----------------------: |

This library is an object-graph mapper with support for LINQ queries. The library
is heavily inspired by the [Entity Framework](https://github.com/dotnet/efcore).

## Getting Started

In your `Program.cs` file register the necessary dependencies:

```csharp
builder.Services.AddNeo4jOGMFactory("connection/uri", IAuthToken, Assembly.GetAssembly(typeof(YourEntity)));
```

The IAuthToken is created from the `Neo4j.Driver` library. Navigate to [their repository](https://github.com/neo4j/neo4j-dotnet-driver) for more information.

To use the library, inject the `SessionFactory` into your class and create a `Session` when needed.

```csharp
...
using var session = sessionFactory.Create();
var entities = session.Set<Entity>().ToListAsync();
```

More can be found in the [example project](https://github.com/TomStary/dotnet-neo4j-ogm-example).

## Contributing

You are welcome to contribute to this library, but before that, please read the "[How to contribute](https://github.com/TomStary/dotnet-neo4j-ogm/blob/main/.github/CONTRIBUTING.md)" page and [code of conduct](https://github.com/TomStary/dotnet-neo4j-ogm/blob/main/.github/CODE_OF_CONDUCT.md).
