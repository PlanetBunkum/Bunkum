using System.Net;
using Bunkum.CustomHttpListener.Request;
using Bunkum.HttpServer.Database;

namespace Bunkum.HttpServer.Authentication;

public interface IAuthenticationProvider<out TUser, out TToken> 
    where TUser : IUser
    where TToken : IToken
{
    // TODO: this is sloppy, figure out how to let auth providers (optionally) choose their own database context
    public TUser? AuthenticateUser(ListenerContext request, Lazy<IDatabaseContext> database);
    public TToken? AuthenticateToken(ListenerContext request, Lazy<IDatabaseContext> database);
}