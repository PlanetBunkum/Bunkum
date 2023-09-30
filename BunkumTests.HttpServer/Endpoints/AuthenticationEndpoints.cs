using Bunkum.Core;
using Bunkum.Core.Authentication.Dummy;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Listener.Parsing;
using HttpMethod = Bunkum.Core.Listener.Parsing.HttpMethod;

namespace BunkumTests.HttpServer.Endpoints;

public class AuthenticationEndpoints : EndpointGroup
{
    [Endpoint("/auth", HttpMethod.Get, ContentType.Json)]
    [Authentication(true)]
    public DummyUser Authentication(RequestContext context, DummyUser user)
    {
        return user;
    }
    
    [Endpoint("/token", HttpMethod.Get, ContentType.Json)]
    [Authentication(true)]
    public string Authentication(RequestContext context, DummyToken token)
    {
        return token.GetType().Name;
    }
}