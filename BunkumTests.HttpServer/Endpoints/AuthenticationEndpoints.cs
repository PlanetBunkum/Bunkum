using Bunkum.Core;
using Bunkum.Core.Authentication.Dummy;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Listener.Parsing;

namespace BunkumTests.HttpServer.Endpoints;

public class AuthenticationEndpoints : EndpointGroup
{
    [Endpoint("/auth", Method.Get, ContentType.Json)]
    [Authentication(true)]
    public DummyUser Authentication(RequestContext context, DummyUser user)
    {
        return user;
    }
    
    [Endpoint("/token", Method.Get, ContentType.Json)]
    [Authentication(true)]
    public string Authentication(RequestContext context, DummyToken token)
    {
        return token.GetType().Name;
    }
}