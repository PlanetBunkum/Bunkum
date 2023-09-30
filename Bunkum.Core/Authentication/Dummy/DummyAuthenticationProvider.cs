using Bunkum.Core.Database;
using Bunkum.Core.Listener.Request;

namespace Bunkum.Core.Authentication.Dummy;

public class DummyAuthenticationProvider : IAuthenticationProvider<DummyToken>
{
    public virtual DummyToken? AuthenticateToken(ListenerContext<Enum, Enum, Enum> request, Lazy<IDatabaseContext> database) =>
        request.RequestHeaders["dummy-skip-auth"] != null ? null : new DummyToken();
}