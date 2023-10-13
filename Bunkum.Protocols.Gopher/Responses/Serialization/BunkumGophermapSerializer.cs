using System.Text;
using Bunkum.Core.Responses;

namespace Bunkum.Protocols.Gopher.Responses.Serialization;

public class BunkumGophermapSerializer : IBunkumSerializer
{
    public string[] ContentTypes => new[]
    {
        GopherContentTypes.Gophermap,
    };
    
    public byte[] Serialize(object data)
    {
        IEnumerable<GophermapItem>? items = data as IEnumerable<GophermapItem>;
        Gophermap? gophermap = data as Gophermap;
        
        if (gophermap == null && items == null)
            throw new InvalidOperationException($"Cannot serialize an object that is not a {nameof(Gophermap)} or {nameof(IEnumerable<GophermapItem>)}");

        items ??= gophermap!.Items;

        StringBuilder str = new();
        foreach (GophermapItem gopherDirectoryItem in items)
        {
            str.Append(gopherDirectoryItem.ItemType);
            str.Append(gopherDirectoryItem.DisplayText);
            
            str.Append('\t');
            str.Append(gopherDirectoryItem.Selector);
            
            str.Append('\t');
            str.Append(gopherDirectoryItem.Hostname);
            
            str.Append('\t');
            str.Append(gopherDirectoryItem.Port);
            
            str.Append('\r');
            str.Append('\n');
        }

        return Encoding.UTF8.GetBytes(str.ToString());
    }
}