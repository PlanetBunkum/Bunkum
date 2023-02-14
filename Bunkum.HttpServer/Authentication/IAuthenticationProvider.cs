using System.Net;
using Bunkum.CustomHttpListener.Request;
using Bunkum.HttpServer.Database;

namespace Bunkum.HttpServer.Authentication;

public interface IAuthenticationProvider<out TUser> where TUser : IUser
{
    // TODO: this is sloppy, figure out how to let auth providers (optionally) choose their own database context
    public TUser? AuthenticateUser(ListenerContext request, IDatabaseContext database);
}