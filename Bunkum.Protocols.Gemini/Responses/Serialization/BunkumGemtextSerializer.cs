using Bunkum.Core.Responses;

namespace Bunkum.Protocols.Gemini.Responses.Serialization;

public class BunkumGemtextSerializer : IBunkumSerializer
{
    public string[] ContentTypes => new[]
    {
        GeminiContentTypes.Gemtext,
    };

    public byte[] Serialize(object data)
    {
        return Array.Empty<byte>();
    }
}