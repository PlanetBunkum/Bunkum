using Amazon.S3;
using Bunkum.Core;
using Bunkum.Core.Configuration;
using Bunkum.DataStores.S3.Configuration;

namespace Bunkum.DataStores.S3.Extensions;

public static class BunkumServerExtensions
{
    public static void AddS3StorageService(this BunkumServer server)
    {
        S3Config config = Config.LoadFromJsonFile<S3Config>("s3.json", server.Logger);

        AmazonS3Config amazonConfig = new()
        {
            ServiceURL = config.ServiceUrl,
        };
        
        AmazonS3Client client = new(config.AccessKeyId, config.AccessKey, amazonConfig);
        
        S3DataStore dataStore = new(client, config.BucketName);
        server.AddStorageService(dataStore);
    }
}