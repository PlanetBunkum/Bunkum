using Bunkum.Core;
using Bunkum.Core.Authentication.Dummy;
using Bunkum.Core.Endpoints;
using Bunkum.Listener.Protocol;

namespace BunkumTests.HttpServer.Endpoints;

public class AuthenticationEndpoints : EndpointGroup
{
    [HttpEndpoint("/auth", HttpMethods.Get, ContentType.Json)]
    [Authentication(true)]
    public DummyUser Authentication(RequestContext context, DummyUser user)
    {
        return user;
    }
    
    [HttpEndpoint("/token", HttpMethods.Get, ContentType.Json)]
    [Authentication(true)]
    public string Authentication(RequestContext context, DummyToken token)
    {
        return token.GetType().Name;
    }
}