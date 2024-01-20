using System.Net;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Responses;
using Bunkum.Core.Storage;
using Bunkum.Protocols.Http;
using HttpMultipartParser;

namespace Bunkum.DataStores.S3.Test;

public class ApiEndpoints : EndpointGroup
{
    [HttpEndpoint("/api/upload", HttpMethods.Post)]
    public Response UploadFile(RequestContext context, Stream body, IDataStore dataStore)
    {
        string? contentType = context.RequestHeaders["Content-Type"];
        if (contentType == null) return HttpStatusCode.BadRequest;

        const string boundaryMarker = "boundary=";
        int boundaryIndex = contentType.IndexOf("boundary=", StringComparison.Ordinal) + boundaryMarker.Length;
        string boundary = contentType.Substring(boundaryIndex);
        Console.WriteLine(boundary);
        
        MultipartFormDataParser? parser = MultipartFormDataParser.Parse(body, boundary);
        if (parser == null) return HttpStatusCode.BadRequest;

        foreach (FilePart? file in parser.Files)
        {
            if(file == null) continue;

            dataStore.WriteToStoreFromStream(file.FileName, file.Data);
        }

        context.ResponseHeaders.Add("Location", "/");
        return HttpStatusCode.Found;
    }

    [HttpEndpoint("/api/download/{key}")]
    [NullStatusCode(HttpStatusCode.NotFound)]
    public byte[]? DownloadFile(RequestContext context, IDataStore dataStore, string key)
    {
        dataStore.TryGetDataFromStore(key, out byte[]? data);
        return data;
    }
}