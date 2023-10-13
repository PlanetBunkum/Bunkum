using Bunkum.Listener.Protocol;
using Newtonsoft.Json;

namespace Bunkum.Core.Responses.Serialization;

/// <summary>
/// An <see cref="IBunkumSerializer"/> that implements serialization for <c>Newtonsoft.JSON</c>-based types
/// </summary>
public class BunkumJsonSerializer : IBunkumSerializer
{
    private static readonly JsonSerializer JsonSerializer = new();

    /// <inherit-doc/>
    public string[] ContentTypes { get; } =
    {
        ContentType.Json,
    };
    
    /// <inherit-doc/>
    public byte[] Serialize(object data)
    {
        using MemoryStream stream = new();
        using StreamWriter sw = new(stream);
        using JsonWriter writer = new JsonTextWriter(sw);

        JsonSerializer.Serialize(writer, data);
        writer.Flush();
        return stream.ToArray();
    }
}