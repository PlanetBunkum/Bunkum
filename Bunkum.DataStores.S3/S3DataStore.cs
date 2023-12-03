using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using Bunkum.Core.Storage;
using static System.Net.HttpStatusCode;

namespace Bunkum.DataStores.S3;

public class S3DataStore(AmazonS3Client client, string bucketName) : IDataStore
{
    private readonly AmazonS3Client _client = client;
    private readonly string _bucketName = bucketName;
    
    // Amazon S3 has a bit more functionality than a dataStore can typically provide.
    // Expose the client to allow users to use it.
    // ReSharper disable once ConvertToAutoPropertyWhenPossible
    public AmazonS3Client Client => this._client;
    
    // https://stackoverflow.com/a/4107867
    public bool ExistsInStore(string key)
    {
        try
        {
            this._client.GetObjectMetadataAsync(this._bucketName, key).Wait();
            return true;
        }
        catch (AggregateException e)
        {
            foreach (Exception innerException in e.InnerExceptions)
            {
                if (innerException is AmazonS3Exception { StatusCode: NotFound }) return false;
            }

            throw;
        }
        catch (AmazonS3Exception e)
        {
            if (e.StatusCode is NotFound) return false;
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
        ListObjectsResponse response = this._client.ListObjectsAsync(this._bucketName).Result;
        return response.S3Objects.Select(o => o.Key).ToArray();
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
        // We can't really create a stream directly into S3 like we can with a file, so leave this unimplemented for now
        throw new NotImplementedException();
    }
}