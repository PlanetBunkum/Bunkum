using Amazon.S3;
using Bunkum.Core.Configuration;

namespace Bunkum.DataStores.S3.Configuration;

public class S3Config : Config
{
    public override int CurrentConfigVersion => 1;
    public override int Version { get; set; }
    protected override void Migrate(int oldVer, dynamic oldConfig)
    {}

    public string BucketName { get; set; } = "bunkum";
    public string ServiceUrl { get; set; } = "http://127.0.0.1:9000";
    
    public string AccessKeyId { get; set; } = "id";
    public string AccessKey { get; set; } = "key";
}