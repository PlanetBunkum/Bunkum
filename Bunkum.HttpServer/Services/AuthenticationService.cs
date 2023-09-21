using System.Net;
using System.Reflection;
using Bunkum.CustomHttpListener.Parsing;
using Bunkum.CustomHttpListener.Request;
using Bunkum.HttpServer.Authentication;
using Bunkum.HttpServer.Authentication.Dummy;
using Bunkum.HttpServer.Database;
using Bunkum.HttpServer.Endpoints;
using Bunkum.HttpServer.Responses;
using NotEnoughLogs;

namespace Bunkum.HttpServer.Services;

public class AuthenticationService : Service
{
    internal AuthenticationService(Logger logger,
        IAuthenticationProvider<IUser, IToken>? provider = null, bool assumeAuthenticationRequired = false) : base(logger)
    {
        this._authenticationProvider = provider ?? new DummyAuthenticationProvider();
        this._assumeAuthenticationRequired = assumeAuthenticationRequired;
    }
    
    /// <summary>
    /// Is authentication required for your endpoints?
    /// If true, clients will receive 403 if your <see cref="IAuthenticationProvider{TUser,TToken}"/> does not return a user.
    /// If false, endpoints will work as normal.
    /// </summary>
    /// <seealso cref="IAuthenticationProvider{TUser, TToken}"/>
    /// <seealso cref="AuthenticationAttribute"/>
    private readonly bool _assumeAuthenticationRequired;
    private readonly IAuthenticationProvider<IUser, IToken> _authenticationProvider;

    public override void Initialize()
    {
        if (this._authenticationProvider is DummyAuthenticationProvider && this._assumeAuthenticationRequired)
        {
            this.Logger.LogWarning(BunkumContext.Startup, "The server was started with a dummy authentication provider. " +
                                                           "If your application relies on authentication, users will always have full access.");
        }
    }
    
    // Cache user objects by the context to avoid double lookups
    private readonly Dictionary<ListenerContext, IUser?> _userCache = new();

    public override Response? OnRequestHandled(ListenerContext context, MethodInfo method, Lazy<IDatabaseContext> database)
    {
        if (!(method.GetCustomAttribute<AuthenticationAttribute>()?.Required ?? this._assumeAuthenticationRequired)) return null;
        
        // Don't need to look in cache, this is the first time we are looking for a user.
        IUser? user = this._authenticationProvider.AuthenticateUser(context, database);
            
        this._userCache.Add(context, user);
            
        if (user == null)
            return new Response(Array.Empty<byte>(), ContentType.Plaintext, HttpStatusCode.Forbidden);

        return null;
    }

    public override object? AddParameterToEndpoint(ListenerContext context, ParameterInfo paramInfo, Lazy<IDatabaseContext> database)
    {
        Type paramType = paramInfo.ParameterType;
        
        if(paramType.IsAssignableTo(typeof(IUser)))
            return this.AuthenticateUser(context, database, true);

        if (paramType.IsAssignableTo(typeof(IToken)))
            return this._authenticationProvider.AuthenticateToken(context, database);

        this._userCache.Remove(context);
        return null;
    }

    public IUser? AuthenticateUser(ListenerContext context, Lazy<IDatabaseContext> database, bool remove = false)
    {
        // Look for the user in the cache.
        // ReSharper disable once InvertIf
        if (this._userCache.TryGetValue(context, out IUser? user))
        {
            if(remove) this._userCache.Remove(context);
            return user;
        }
            
        return this._authenticationProvider.AuthenticateUser(context, database);
    }
}