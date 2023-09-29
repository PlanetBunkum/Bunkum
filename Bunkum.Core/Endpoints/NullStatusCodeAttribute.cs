using System.Net;

namespace Bunkum.Core.Endpoints;

/// <summary>
/// When returning null as a response, the default is 404 NotFound.
/// You can use this attribute to replace that endpoint's default status code when the response is null.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class NullStatusCodeAttribute : Attribute
{
    public readonly HttpStatusCode StatusCode;

    public NullStatusCodeAttribute(HttpStatusCode statusCode)
    {
        this.StatusCode = statusCode;
    }
}