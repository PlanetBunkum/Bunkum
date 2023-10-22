using System.Buffers.Text;
using System.Net;
using Bunkum.Listener.Request;
using Bunkum.Protocols.Gemini.Responses;

namespace Bunkum.Protocols.Gemini.Socket;

public class SocketGeminiListenerContext : SocketListenerContext
{
    public SocketGeminiListenerContext(System.Net.Sockets.Socket socket, Stream stream) : base(socket, stream)
    {}

    /// <inheritdoc />
    public override long ContentLength => 0;
   
    //This is stored statically to save on allocations
    private static readonly byte[] LineEnding = "\r\n"u8.ToArray();
    
    protected override async Task SendResponseInternal(HttpStatusCode code, ArraySegment<byte>? data = null)
    {
        GeminiStatusCode statusCode = code.ToGemini();

        //Allocate 3 bytes, 2 for the status code, one for the space
        byte[] statusCodeBytes = new byte[3];
        if (!Utf8Formatter.TryFormat((int)statusCode, statusCodeBytes, out int written))
            throw new Exception("Unable to format status code");
        //Status codes are 2 characters long in Gemini
        if (written != 2)
            throw new Exception("Gemini status code *must* be >= 2 characters in length in decimal.");
        //Set the third char to a space
        statusCodeBytes[2] = (byte)' ';
        
        //Send the status code with a space after
        await this.SendBufferSafe(statusCodeBytes);
        
        //When its a non-success status code, send the data as the META, else send the response type as the MIME
        if (statusCodeBytes[0] != (byte)'2' && data != null)
            await this.SendBufferSafe(data.Value);
        else
            await this.SendBufferSafe(this.ResponseType ?? GeminiContentTypes.Gemtext);
        await this.SendBufferSafe(new ArraySegment<byte>(LineEnding));
        
        //Only send body data if its a success response
        if (statusCodeBytes[0] == (byte)'2' && data != null)
            await this.SendBufferSafe(data.Value);
    }
}