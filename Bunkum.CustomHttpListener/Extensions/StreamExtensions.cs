namespace Bunkum.CustomHttpListener.Extensions;

internal static class StreamExtensions
{
    private static async Task<int> ReadByteAsync(this Stream stream, CancellationToken ct)
    {
        byte[] oneByteArray = new byte[1];
        int r = await stream.ReadAsync(oneByteArray, 0, 1, ct);
        return r == 0 ? -1 : oneByteArray[0];
    }

    private static async Task WriteByteAsync(this Stream stream, byte value, CancellationToken ct)
    {
        await stream.WriteAsync(new byte[1] { value }, 0, 1, ct);
    }
    
    internal static async Task<int> ReadIntoBufferUntilCharAsync(this Stream stream, char charToReadTo, byte[] buffer, CancellationToken ct)
    {
        int readByte;
        int i = 0;
        while ((readByte = await stream.ReadByteAsync(ct)) != -1)
        {
            if ((char)readByte == charToReadTo) break;

            buffer[i] = (byte)readByte;
            i++;
        }

        return i;
    }

    internal static async Task<int> ReadIntoStreamAsync(this Stream stream, Stream otherStream, int count, CancellationToken ct)
    {
        int i = 0;
        while(i < count)
        {
            int readByte = await stream.ReadByteAsync(ct);

            await otherStream.WriteByteAsync((byte)readByte, ct);
            i++;
        }

        return i; 
    }
}