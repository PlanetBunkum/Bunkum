using System.Collections.ObjectModel;
using System.Text;
using Bunkum.Core.Responses;

namespace Bunkum.Protocols.Gopher.Responses.Serialization;

public class BunkumGophermapSerializer : IBunkumSerializer
{
    public string[] ContentTypes => new[]
    {
        GopherContentTypes.Gophermap,
    };
    
    // https://stackoverflow.com/a/1450889
    private static IEnumerable<string> SplitIntoChunks(string str, int maxChunkSize) {
        for (int i = 0; i < str.Length; i += maxChunkSize) 
            yield return str.Substring(i, Math.Min(maxChunkSize, str.Length - i));
    }

    
    public byte[] Serialize(object data)
    {
        IEnumerable<GophermapItem>? items = data as IEnumerable<GophermapItem>;
        Gophermap? gophermap = data as Gophermap;
        
        if (gophermap == null && items == null)
            throw new InvalidOperationException($"Cannot serialize an object that is not a {nameof(Gophermap)} or {nameof(IEnumerable<GophermapItem>)}");

        items ??= gophermap!.Items;
        items = items.ToList();

        StringBuilder str = new();

        int i = 0;
        int count = items.Count();
        foreach (GophermapItem gopherDirectoryItem in items)
        {
            foreach (string chunk in SplitIntoChunks(gopherDirectoryItem.DisplayText, 70))
            {
                str.Append(gopherDirectoryItem.ItemType);
                str.Append(chunk);
            
                str.Append('\t');
                str.Append(gopherDirectoryItem.Selector);
            
                str.Append('\t');
                str.Append(gopherDirectoryItem.Hostname);
            
                str.Append('\t');
                str.Append(gopherDirectoryItem.Port);

                if (i != count)
                {
                    str.Append("\r\n");
                }
            }

            i++;
        }

        str.Append(".\r\n");

        return Encoding.UTF8.GetBytes(str.ToString());
    }
}