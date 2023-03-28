using System.Net;

namespace Bunkum.HttpServer.Endpoints;

/// <summary>
/// When returning an object as a response, the default is 200 OK.
/// You can use this attribute to replace that endpoint's default status code.
/// </summary>
public class SuccessStatusCodeAttribute : Attribute
{
    public readonly HttpStatusCode StatusCode;

    public SuccessStatusCodeAttribute(HttpStatusCode statusCode)
    {
        this.StatusCode = statusCode;
    }
}