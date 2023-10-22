using System.Text;
using Bunkum.Core.Responses;

namespace Bunkum.Protocols.Gopher.Responses.Serialization;

/// <summary>
/// An <see cref="IBunkumSerializer"/> that implements serialization for <see cref="Gophermap"/>s.
/// Will also accept <see cref="IEnumerable&lt;GophermapItem&gt;"/>s.
/// </summary>
public class BunkumGophermapSerializer : IBunkumSerializer
{
    /// <inherit-doc/>
    public string[] ContentTypes => new[]
    {
        GopherContentTypes.Gophermap,
    };
    
    // Adapted from https://stackoverflow.com/a/1450889
    private static List<string> SplitIntoChunks(string str, int maxChunkSize)
    {
        if (str.Length <= maxChunkSize)
        {
            return new List<string>(1) { str };
        }
        
        // Add 1 since the string wont always fit exactly to a multiple of maxChunkSize
        List<string> chunks = new(str.Length / maxChunkSize + 1);
        
        for (int i = 0; i < str.Length; i += maxChunkSize) 
            chunks.Add(str.Substring(i, Math.Min(maxChunkSize, str.Length - i)));
        
        return chunks;
    }

    
    /// <inherit-doc/>
    public byte[] Serialize(object data)
    {
        IEnumerable<GophermapItem>? items = data as IEnumerable<GophermapItem>;
        Gophermap? gophermap = data as Gophermap;
        
        if (gophermap == null && items == null)
            throw new InvalidOperationException($"Cannot serialize an object that is not a {nameof(Gophermap)} or {nameof(IEnumerable<GophermapItem>)}");

        items ??= gophermap!.Items;
        items = items.ToList(); // Convert to a list so we avoid multiple enumeration

        StringBuilder str = new();

        int i = 0;
        int count = items.Count();
        foreach (GophermapItem gopherDirectoryItem in items)
        {
            List<string> chunks = SplitIntoChunks(gopherDirectoryItem.DisplayText, 70);
            for (int chunkIndex = 0; chunkIndex < chunks.Count; chunkIndex++)
            {
                string displayTextChunk = chunks[chunkIndex];
                
                str.Append((char)gopherDirectoryItem.ItemType);
                str.Append(displayTextChunk);

                str.Append('\t');
                str.Append(gopherDirectoryItem.Selector);

                str.Append('\t');
                str.Append(gopherDirectoryItem.Hostname);

                str.Append('\t');
                str.Append(gopherDirectoryItem.Port);

                // IF we're not yet at the end,
                // OR we're in the middle of printing a series of chunks,
                // THEN add a new line.
                if (i != count || chunkIndex != chunks.Count)
                {
                    str.Append("\r\n");
                }
            }

            i++;
        }

        // Gopher clients expect a trailing period referred to as the Lastline
        str.Append(".\r\n");

        return Encoding.UTF8.GetBytes(str.ToString());
    }
}