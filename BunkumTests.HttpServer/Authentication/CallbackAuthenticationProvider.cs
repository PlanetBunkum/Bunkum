using System.Net;
using Bunkum.CustomHttpListener.Request;
using Bunkum.HttpServer.Authentication.Dummy;
using Bunkum.HttpServer.Database;

namespace BunkumTests.HttpServer.Authentication;

public class CallbackAuthenticationProvider : DummyAuthenticationProvider
{
    private readonly Action _action;

    public CallbackAuthenticationProvider(Action action)
    {
        this._action = action;
    }

    public override DummyToken? AuthenticateToken(ListenerContext request, Lazy<IDatabaseContext> database)
    {
        this._action.Invoke();
        return base.AuthenticateToken(request, database);
    }
}