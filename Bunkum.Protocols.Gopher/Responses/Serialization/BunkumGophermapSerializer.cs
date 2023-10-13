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
        if (data is not Gophermap gophermap)
            throw new InvalidOperationException($"Cannot serialize an object that is not a {nameof(Gophermap)}");

        StringBuilder str = new();
        foreach (GophermapItem gopherDirectoryItem in gophermap.Items)
        {
            str.Append(gopherDirectoryItem.ItemType);
            str.Append(gopherDirectoryItem.DisplayString);
            
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