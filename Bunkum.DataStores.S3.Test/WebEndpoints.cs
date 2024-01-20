using System.Diagnostics;
using System.Text;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Storage;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;

namespace Bunkum.DataStores.S3.Test;

public class WebEndpoints : EndpointGroup
{
    [HttpEndpoint("/", ContentType.Html)]
    public string GetIndex(RequestContext context, IDataStore dataStore)
    {
        Stopwatch stopwatch = new();
        StringBuilder keys = new();
        
        stopwatch.Start();
        foreach (string key in dataStore.GetKeysFromStore())
        {
            keys.Append($"<a href=\"/api/download/{key}\">{key}</a><br>");
        }
        stopwatch.Stop();
        
        return
            $"""
            <!DOCTYPE html>
            <html>
            <body>
            <h1>Bunkum S3 DataStore Test</h1>
            <p>Key retrieval took {stopwatch.ElapsedMilliseconds}ms.</p>
            
            <h2>Upload</h2>
            <form action="/api/upload" method="post" enctype="multipart/form-data">
                <label for="file">Select a file:</label>
                <input type="file" name="file" id="file" required>
                <br>
                <input type="submit" value="Upload">
            </form>
            
            <h2>Keys in IDataStore</h2>
            {keys}
            
            </body>
            </html>
            """;
    }
}