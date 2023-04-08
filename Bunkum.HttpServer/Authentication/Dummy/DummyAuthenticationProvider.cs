using System.Net;
using Bunkum.CustomHttpListener.Request;
using Bunkum.HttpServer.Database;

namespace Bunkum.HttpServer.Authentication.Dummy;

public class DummyAuthenticationProvider : IAuthenticationProvider<DummyUser, DummyToken>
{
    public virtual DummyUser? AuthenticateUser(ListenerContext request, Lazy<IDatabaseContext> database) => 
        request.RequestHeaders["dummy-skip-auth"] != null ? null : new DummyUser();

    public DummyToken? AuthenticateToken(ListenerContext request, Lazy<IDatabaseContext> database) =>
        request.RequestHeaders["dummy-skip-auth"] != null ? null : new DummyToken();
}