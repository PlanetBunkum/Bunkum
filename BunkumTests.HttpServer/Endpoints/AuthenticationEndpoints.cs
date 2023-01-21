using Bunkum.HttpServer;
using Bunkum.HttpServer.Authentication.Dummy;
using Bunkum.HttpServer.Endpoints;
using Bunkum.HttpServer.Responses;

namespace BunkumTests.HttpServer.Endpoints;

public class AuthenticationEndpoints : EndpointGroup
{
    [Endpoint("/auth", Method.Get, ContentType.Json)]
    [Authentication(true)]
    public DummyUser Authentication(RequestContext context, DummyUser user)
    {
        return user;
    }
}