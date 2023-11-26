using System.Reflection;
using System.Text;
using Amazon.S3;
using Amazon.S3.Model;
using Bunkum.DataStores.S3;
using Bunkum.Protocols.Http;

AmazonS3Config config = new()
{
    ServiceURL = "http://127.0.0.1:9000",
};

AmazonS3Client client = new("minioadmin", "minioadmin", config);
S3DataStore dataStore = new(client, "bunkum");
    
foreach (S3Bucket bucket in client.ListBucketsAsync().Result.Buckets)
{
    Console.WriteLine(bucket.BucketName);
}
    
dataStore.WriteToStore("test", "data"u8.ToArray());
Console.WriteLine(Encoding.ASCII.GetString(dataStore.GetDataFromStore("test")));

return;

BunkumHttpServer server = new();

server.Initialize = s =>
{

    
    // s.AddStorageService(dataStore);
    s.DiscoverEndpointsFromAssembly(Assembly.GetExecutingAssembly());
};

server.Start();
await Task.Delay(-1);