using System.Text;
using Bunkum.Core.Responses;
using Bunkum.Protocols.Gemini.Responses;
using Bunkum.Protocols.Gopher.Responses;

namespace Bunkum.Serialization.GopherToGemini;

/// <summary>
/// An <see cref="IBunkumSerializer"/> that implements serialization of <see cref="Gophermap"/>s for Gemini clients.
/// Will also accept <see cref="IEnumerable&lt;GophermapItem&gt;"/>s.
/// </summary>
public class BunkumGophermapGeminiSerializer : IBunkumSerializer
{
    public string[] ContentTypes { get; } =
    {
        GeminiContentTypes.Gemtext,
    };
    
    public byte[] Serialize(object data)
    {
        IEnumerable<GophermapItem>? items = data as IEnumerable<GophermapItem>;
        Gophermap? gophermap = data as Gophermap;
        
        if (gophermap == null && items == null)
            throw new InvalidOperationException($"Cannot serialize an object that is not a {nameof(Gophermap)} or {nameof(IEnumerable<GophermapItem>)}");

        items ??= gophermap!.Items;
        items = items.ToList(); // Convert to a list so we avoid multiple enumeration

        StringBuilder str = new();
        
        foreach (GophermapItem gopherDirectoryItem in items)
        {
            switch (gopherDirectoryItem.ItemType)
            {
                case GophermapItemType.Message:
                case GophermapItemType.Error:
                {
                    str.Append(gopherDirectoryItem.DisplayText);
                    break;
                }
                case GophermapItemType.File:
                case GophermapItemType.Directory:
                default:
                {
                    str.Append($"=> gemini://{gopherDirectoryItem.Hostname}:{gopherDirectoryItem.Port}{gopherDirectoryItem.Selector} {gopherDirectoryItem.DisplayText}");
                    break;
                }
            }
            
            str.Append("\r\n");
        }

        return Encoding.UTF8.GetBytes(str.ToString());
    }
}