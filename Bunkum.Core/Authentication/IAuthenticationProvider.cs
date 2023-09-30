using Bunkum.Core.Database;
using Bunkum.Listener.Request;

namespace Bunkum.Core.Authentication;

public interface IAuthenticationProvider<out TToken> where TToken : IToken<IUser>
{
    // TODO: this is sloppy, figure out how to let auth providers (optionally) choose their own database context
    public TToken? AuthenticateToken(ListenerContext request, Lazy<IDatabaseContext> database);
}