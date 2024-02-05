# Bunkum.DataStores.S3

A package for Bunkum that adds native support for S3 object storage.

This package is new and experimental.
Please create issues on our [issue tracker](https://github.com/PlanetBunkum/Bunkum/issues) for questions, support and ...well, issues.

## Usage

Install the package, and add it to your `Initialize` function:
```csharp
BunkumHttpServer server = new();

server.Initialize = s =>
{
    s.AddS3StorageService();
    s.DiscoverEndpointsFromAssembly(Assembly.GetExecutingAssembly());
};
```

On the next startup, Bunkum will create a `s3.json` file for you to configure.

You can also create the `S3DataStore` class manually to inject your own `AmazonS3Client`.