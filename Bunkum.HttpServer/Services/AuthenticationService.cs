using Bunkum.CustomHttpListener.Request;
using Bunkum.HttpServer.Authentication;
using Bunkum.HttpServer.Authentication.Dummy;
using Bunkum.HttpServer.Database;
using Bunkum.HttpServer.Endpoints;
using NotEnoughLogs;

namespace Bunkum.HttpServer.Services;

public class AuthenticationService : Service
{
    internal AuthenticationService(LoggerContainer<BunkumContext> logger,
        IAuthenticationProvider<IUser, IToken>? provider = null, bool assumeAuthenticationRequired = false) : base(logger)
    {
        this._authenticationProvider = provider ?? new DummyAuthenticationProvider();
        this.AssumeAuthenticationRequired = assumeAuthenticationRequired;
    }
    
    /// <summary>
    /// Is authentication required for your endpoints?
    /// If true, clients will receive 403 if your <see cref="IAuthenticationProvider{TUser,TToken}"/> does not return a user.
    /// If false, endpoints will work as normal.
    /// </summary>
    /// <seealso cref="IAuthenticationProvider{TUser, TToken}"/>
    /// <seealso cref="AuthenticationAttribute"/>
    internal readonly bool AssumeAuthenticationRequired;
    private readonly IAuthenticationProvider<IUser, IToken> _authenticationProvider;

    public override void Initialize()
    {
        if (this._authenticationProvider is DummyAuthenticationProvider && this.AssumeAuthenticationRequired)
        {
            this.Logger.LogWarning(BunkumContext.Startup, "The server was started with a dummy authentication provider. " +
                                                           "If your application relies on authentication, users will always have full access.");
        }
    }

    public IUser? AuthenticateUser(ListenerContext request, Lazy<IDatabaseContext> database)
    {
        return this._authenticationProvider.AuthenticateUser(request, database);
    }

    public IToken? AuthenticateToken(ListenerContext request, Lazy<IDatabaseContext> database)
    {
        return this._authenticationProvider.AuthenticateToken(request, database);
    }
}