using System.Reflection;
using Bunkum.DataStores.S3.Extensions;
using Bunkum.Protocols.Http;

BunkumHttpServer server = new();

server.Initialize = s =>
{
    s.AddS3StorageService();
    s.DiscoverEndpointsFromAssembly(Assembly.GetExecutingAssembly());
};

server.Start();
await Task.Delay(-1);