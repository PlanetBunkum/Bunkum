using System.Net;

namespace Bunkum.HttpServer.Endpoints;

[AttributeUsage(AttributeTargets.Method)]
public class NullStatusCodeAttribute : Attribute
{
    public readonly HttpStatusCode StatusCode;

    public NullStatusCodeAttribute(HttpStatusCode statusCode)
    {
        this.StatusCode = statusCode;
    }
}