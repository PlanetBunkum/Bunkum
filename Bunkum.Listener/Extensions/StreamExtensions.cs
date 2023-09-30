namespace Bunkum.Listener.Extensions;

internal static class StreamExtensions
{
    internal static int ReadIntoBufferUntilChar(this Stream stream, char charToReadTo, Span<byte> buffer)
    {
        int readByte;
        int i = 0;
        while ((readByte = stream.ReadByte()) != -1)
        {
            if ((char)readByte == charToReadTo) break;

            buffer[i] = (byte)readByte;
            i++;
        }

        return i;
    }
    
    internal static int ReadIntoBufferUntilChar(this Stream stream, char charToReadTo, Span<char> buffer)
    {
        int readByte;
        int i = 0;
        while ((readByte = stream.ReadByte()) != -1)
        {
            char readChar = (char)readByte;
            if (readChar == charToReadTo) break;

            buffer[i] = readChar;
            i++;
        }

        return i;
    }
    
    internal static int SkipBufferUntilChar(this Stream stream, char charToReadTo)
    {
        int readByte;
        int i = 0;
        while ((readByte = stream.ReadByte()) != -1)
        {
            if ((char)readByte == charToReadTo) break;
            i++;
        }

        return i;
    }

    internal static int ReadIntoStream(this Stream stream, Stream otherStream, int count)
    {
        int i = 0;
        while(i < count)
        {
            int readByte = stream.ReadByte();

            otherStream.WriteByte((byte)readByte);
            i++;
        }

        return i; 
    }
}