using System.Net;
using System.Text;
using Bunkum.Core.Responses.Serialization;
using Bunkum.Listener.Protocol;

namespace Bunkum.Core.Responses;

public partial struct Response
{
    // FIXME: storing this statically is beyond moronic especially given the case of unit tests / multiple servers per process in general,
    // but my hand is forced as we must allow the user to create their own Response objects cleanly
    // we could just pass in the context but MEH, that's a later jvyden problem
    // yeah that's right fuck you later jvyden
    private static readonly List<IBunkumSerializer> Serializers = new();
    
    /// <summary>
    /// Registers a <see cref="IBunkumSerializer"/> that will be used for a set of Content Types when creating a Response's data.
    /// </summary>
    /// <param name="serializer">The serializer to use</param>
    public static void AddSerializer(IBunkumSerializer serializer)
    {
        foreach (string contentType in serializer.ContentTypes)
        {
            if (GetSerializerOrDefault(contentType) != null)
                throw new InvalidOperationException($"Cannot add a serializer when there is already a serializer handling '{contentType}'");
        }

        Serializers.Add(serializer);
    }

    /// <summary>
    /// Registers a <see cref="TBunkumSerializer"/> that will be used for its set of Content Types when creating a Response's data.
    /// </summary>
    /// <typeparam name="TBunkumSerializer">The type of <see cref="IBunkumSerializer"/> to create.</typeparam>
    public static void AddSerializer<TBunkumSerializer>() where TBunkumSerializer : IBunkumSerializer, new()
    {
        AddSerializer(new TBunkumSerializer());
    }

    private static IBunkumSerializer? GetSerializerOrDefault(string contentType) 
        => Serializers.FirstOrDefault(s => s.ContentTypes.Contains(contentType));

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
        AddSerializer<BunkumJsonSerializer>();
        AddSerializer<BunkumXmlSerializer>();
    }

    private static partial void SetupResponseCache();

    public Response(HttpStatusCode statusCode) : this("", ContentType.BinaryData, statusCode) 
    {}

    public Response(object? data, string contentType = ContentType.Plaintext, HttpStatusCode statusCode = HttpStatusCode.OK, bool skipSerialization = false)
    {
        this.StatusCode = statusCode;

        IBunkumSerializer? serializer = GetSerializerOrDefault(contentType);
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