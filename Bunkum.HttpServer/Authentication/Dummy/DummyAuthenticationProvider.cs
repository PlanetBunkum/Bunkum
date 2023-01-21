using System.Net;
using Bunkum.HttpServer.Database;

namespace Bunkum.HttpServer.Authentication.Dummy;

public class DummyAuthenticationProvider : IAuthenticationProvider<DummyUser>
{
    public virtual DummyUser? AuthenticateUser(HttpListenerRequest request, IDatabaseContext database) => 
        request.Headers["dummy-skip-auth"] != null ? null : new DummyUser();
}