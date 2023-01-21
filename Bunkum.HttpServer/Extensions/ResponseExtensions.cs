using System.Net;
using System.Text;

namespace Bunkum.HttpServer.Extensions;

public static class ResponseExtensions
{
    public static void WriteString(this HttpListenerResponse resp, string data) 
        => resp.OutputStream.Write(Encoding.Default.GetBytes(data));
}