using System.Net;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Bunkum.HttpServer.Responses;
using Bunkum.HttpServer.Storage;

namespace BunkumTests.HttpServer.Endpoints;

public class StorageEndpoints : EndpointGroup
{
    [Endpoint("/storage/put")]
    public Response Put(RequestContext context, IDataStore dataStore)
    {
        dataStore.WriteToStore("file", "data"u8);
        return HttpStatusCode.OK;
    }

    [Endpoint("/storage/get")]
    public byte[] Get(RequestContext context, IDataStore dataStore) => dataStore.GetDataFromStore("file");
}