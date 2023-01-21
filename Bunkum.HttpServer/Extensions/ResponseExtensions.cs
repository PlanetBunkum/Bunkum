using System.Net;
using System.Text;

namespace Bunkum.HttpServer.Extensions;

internal static class ResponseExtensions
{
    internal static void WriteString(this HttpListenerResponse resp, string data) 
        => resp.OutputStream.Write(Encoding.Default.GetBytes(data));
}