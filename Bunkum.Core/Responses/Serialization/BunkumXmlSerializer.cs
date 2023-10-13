using System.Xml.Serialization;
using Bunkum.Listener.Protocol;

namespace Bunkum.Core.Responses.Serialization;

/// <summary>
/// An <see cref="IBunkumSerializer"/> that implements serialization for <c>System.Xml.Serialization</c>-based types
/// </summary>
public class BunkumXmlSerializer : IBunkumSerializer
{
    static BunkumXmlSerializer()
    {
        Namespaces.Add("", "");
    }
    
    private static readonly XmlSerializerNamespaces Namespaces = new();
    
    /// <inherit-doc/>
    public string[] ContentTypes { get; } =
    {
        ContentType.Xml,
    };
    
    /// <inherit-doc/>
    public byte[] Serialize(object data)
    {
        using MemoryStream stream = new();
        using BunkumXmlTextWriter writer = new(stream);

        XmlSerializer serializer = new(data.GetType());
        serializer.Serialize(writer, data, Namespaces);

        return stream.ToArray();
    }
}