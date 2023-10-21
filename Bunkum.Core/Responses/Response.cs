using System.Net;
using System.Text;
using Bunkum.Core.Responses.Serialization;
using Bunkum.Listener.Protocol;

namespace Bunkum.Core.Responses;

public partial struct Response
{
    private static readonly List<IBunkumSerializer> Serializers = new();

    public readonly HttpStatusCode StatusCode;
    public readonly string ResponseType;
    public readonly byte[] Data;
    
    public Response(byte[] data, string contentType = ContentType.Plaintext, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        this.StatusCode = statusCode;
        this.Data = data;
        this.ResponseType = contentType;
    }

    static Response()
    {
        SetupResponseCache();
        Serializers.Add(new BunkumJsonSerializer());
        Serializers.Add(new BunkumXmlSerializer());
    }

    private static partial void SetupResponseCache();

    public Response(HttpStatusCode statusCode) : this(string.Empty, ContentType.BinaryData, statusCode) 
    {}

    public Response(object? data, string contentType = ContentType.Plaintext, HttpStatusCode statusCode = HttpStatusCode.OK, IBunkumSerializer? serializer = null, bool skipSerialization = false)
    {
        this.StatusCode = statusCode;
        this.ResponseType = contentType;

        serializer ??= Serializers.FirstOrDefault(s => s.ContentTypes.Contains(contentType));
        if (skipSerialization || serializer is null || data is null or string)
        {
            this.Data = Encoding.Default.GetBytes(data?.ToString() ?? string.Empty);
            return;
        }
        
        if (data is IHasResponseCode status)
            this.StatusCode = status.StatusCode;

        this.Data = serializer.Serialize(data);
    }
}