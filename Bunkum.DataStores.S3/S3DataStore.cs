using Amazon.S3;
using Amazon.S3.Model;
using Bunkum.Core.Storage;
using static System.Net.HttpStatusCode;

namespace Bunkum.DataStores.S3;

public class S3DataStore(AmazonS3Client client, string bucketName) : IDataStore
{
    private readonly AmazonS3Client _client = client;
    private readonly string _bucketName = bucketName;

    // https://stackoverflow.com/a/4107867
    public bool ExistsInStore(string key)
    {
        try
        {
            this._client.GetObjectMetadataAsync(this._bucketName, key).Wait();
            return true;
        }
        catch(AmazonS3Exception e)
        {
            if (e.StatusCode == NotFound) return false;
            throw;
        }
    }

    public bool WriteToStore(string key, byte[] data)
    {
        using MemoryStream ms = new(data);
        return this.WriteToStoreFromStream(key, ms);
    }

    public byte[] GetDataFromStore(string key)
    {
        Stream data = this.GetStreamFromStore(key);
        byte[] buffer = new byte[data.Length];
        _ = data.Read(buffer, 0, (int)data.Length);

        return buffer;
    }

    public bool RemoveFromStore(string key)
    {
        DeleteObjectResponse response = this._client.DeleteObjectAsync(this._bucketName, key).Result;
        return response.HttpStatusCode == OK;
    }

    public string[] GetKeysFromStore()
    {
        throw new NotImplementedException();
    }

    public bool WriteToStoreFromStream(string key, Stream data)
    {
        PutObjectRequest request = new()
        {
            Key = key,
            BucketName = this._bucketName,
            InputStream = data,
        };

        PutObjectResponse response = this._client.PutObjectAsync(request).Result;
        return response.HttpStatusCode == OK;
    }

    public Stream GetStreamFromStore(string key)
    {
        GetObjectResponse response = this._client.GetObjectAsync(this._bucketName, key).Result;
        return response.ResponseStream;
    }

    public Stream OpenWriteStream(string key)
    {
        throw new NotImplementedException();
    }
}