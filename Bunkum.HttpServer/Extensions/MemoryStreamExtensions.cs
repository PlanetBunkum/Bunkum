using System.Text;

namespace Bunkum.HttpServer.Extensions;

internal static class MemoryStreamExtensions
{
    internal static void WriteString(this MemoryStream ms, string str) => ms.Write(Encoding.Default.GetBytes(str));
}