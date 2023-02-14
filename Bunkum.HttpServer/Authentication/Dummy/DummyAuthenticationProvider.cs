using System.Net;
using Bunkum.CustomHttpListener.Request;
using Bunkum.HttpServer.Database;

namespace Bunkum.HttpServer.Authentication.Dummy;

public class DummyAuthenticationProvider : IAuthenticationProvider<DummyUser>
{
    public virtual DummyUser? AuthenticateUser(ListenerContext request, IDatabaseContext database) => 
        request.RequestHeaders["dummy-skip-auth"] != null ? null : new DummyUser();
}