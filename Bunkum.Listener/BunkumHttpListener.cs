using System.Text;
using Bunkum.Listener.Extensions;
using NotEnoughLogs;

namespace Bunkum.Listener;

public abstract class BunkumHttpListener : BunkumListener
{
    public BunkumHttpListener(Logger logger) : base(logger)
    {}
    
    public static IEnumerable<(string, string)> ReadCookies(ReadOnlySpan<char> cookieHeader)
    {
        List<(string key, string value)> cookies = new(10);

        bool parsedName = false;
        
        int nameIndex = 0;
        int dataIndex = 0;
        int startIndex = 0;
        
        for (int cookieIndex = 0; cookieIndex < cookieHeader.Length; cookieIndex++)
        {
            char c = cookieHeader[cookieIndex];
            if (!parsedName)
            {
                if (c != '=')
                {
                    nameIndex = cookieIndex + 1;
                    continue;
                }
                
                parsedName = true;
                dataIndex = cookieIndex;
            }


            bool isLastByte = cookieIndex == cookieHeader.Length - 1;
            if (c == ';' || isLastByte)
            {
                if (isLastByte) dataIndex++;

                ReadOnlySpan<char> nameSlice = cookieHeader[startIndex..nameIndex].TrimStart();
                ReadOnlySpan<char> dataSlice = cookieHeader[(nameIndex + 1)..dataIndex].TrimEnd();
                
                cookies.Add((nameSlice.ToString(), dataSlice.ToString()));
                startIndex = cookieIndex + 1;
                parsedName = false;
            }
            else
            {
                dataIndex++;
            }
        }

        return cookies;
    }

    public static IEnumerable<(string key, string value)> ReadHeaders(Stream stream)
    {
        List<(string key, string value)> headers = new(10);
        Span<byte> headerLineBytes = stackalloc byte[HeaderLineLimit];
        
        while (true)
        {
            int count = stream.ReadIntoBufferUntilChar('\r', headerLineBytes);
            stream.ReadByte(); // Skip \n

            string headerLine = Encoding.UTF8.GetString(headerLineBytes[..count]);
            int index = headerLine.IndexOf(": ", StringComparison.Ordinal);
            if(index == -1) break; // no more headers

            string key = headerLine.Substring(0, index);
            string value = headerLine.Substring(index + 2);

            headers.Add((key, value));
        }

        return headers;
    }
}