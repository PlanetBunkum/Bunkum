using Bunkum.CustomHttpListener.Request;
using Bunkum.HttpServer.Database;

namespace Bunkum.HttpServer.Authentication;

public interface IAuthenticationProvider<out TToken> where TToken : IToken<IUser>
{
    // TODO: this is sloppy, figure out how to let auth providers (optionally) choose their own database context
    public TToken? AuthenticateToken(ListenerContext request, Lazy<IDatabaseContext> database);
}