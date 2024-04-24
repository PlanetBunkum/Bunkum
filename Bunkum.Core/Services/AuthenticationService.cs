using System.Net;
using System.Reflection;
using Bunkum.Core.Authentication;
using Bunkum.Core.Authentication.Dummy;
using Bunkum.Core.Database;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Responses;
using Bunkum.Listener.Protocol;
using Bunkum.Listener.Request;
using NotEnoughLogs;

namespace Bunkum.Core.Services;

public class AuthenticationService : Service
{
    internal AuthenticationService(Logger logger,
        IAuthenticationProvider<IToken<IUser>>? provider = null, bool assumeAuthenticationRequired = false) : base(logger)
    {
        this._authenticationProvider = provider ?? new DummyAuthenticationProvider();
        this._assumeAuthenticationRequired = assumeAuthenticationRequired;
    }
    
    /// <summary>
    /// Is authentication required for your endpoints?
    /// If true, clients will receive 403 if your <see cref="IAuthenticationProvider{TToken}"/> does not return a user.
    /// If false, endpoints will work as normal.
    /// </summary>
    /// <seealso cref="IAuthenticationProvider{TToken}"/>
    /// <seealso cref="AuthenticationAttribute"/>
    private readonly bool _assumeAuthenticationRequired;
    private readonly IAuthenticationProvider<IToken<IUser>> _authenticationProvider;

    public override void Initialize()
    {
        if (this._authenticationProvider is DummyAuthenticationProvider && this._assumeAuthenticationRequired)
        {
            this.Logger.LogWarning(BunkumCategory.Startup, "The server was started with a dummy authentication provider. " +
                                                           "If your application relies on authentication, users will always have full access.");
        }
    }
    
    // Cache the token to avoid double lookups.
    // We don't use a list here as you can't have multiple tokens per request (and thus one token per thread)
    private readonly ThreadLocal<IToken<IUser>?> _tokenCache = new(() => null);

    /// <inheritdoc />
    public override Response? OnRequestHandled(ListenerContext context, MethodInfo method, Lazy<IDatabaseContext> database)
    {
        if (!(method.GetCustomAttribute<AuthenticationAttribute>()?.Required ?? this._assumeAuthenticationRequired)) return null;
        
        IToken<IUser>? token = this.AuthenticateToken(context, database);
        this._tokenCache.Value = token;
            
        if (token == null)
            return new Response(Array.Empty<byte>(), ContentType.Plaintext, HttpStatusCode.Forbidden);

        return null;
    }

    /// <inheritdoc />
    public override object? AddParameterToEndpoint(ListenerContext context, BunkumParameterInfo parameter, Lazy<IDatabaseContext> database)
    {
        if(ParameterBasedFrom<IToken<IUser>>(parameter))
            return this.AuthenticateToken(context, database);

        if (ParameterBasedFrom<IUser>(parameter))
        {
            IToken<IUser>? token = this.AuthenticateToken(context, database);
            if (token != null) return token.User;
        }
        
        return null;
    }

    /// <inheritdoc />
    public override void AfterRequestHandled(ListenerContext context, Response response, MethodInfo method, Lazy<IDatabaseContext> database)
    {
        this._tokenCache.Value = null;
    }

    public IToken<IUser>? AuthenticateToken(ListenerContext context, Lazy<IDatabaseContext> database, bool remove = false)
    {
        this.Logger.LogTrace(nameof(AuthenticationService), "Attempting to look up a token in the cache...");
        
        // Look for the user in the cache.
        // ReSharper disable once InvertIf
        if (this._tokenCache.Value != null)
        {
            this.Logger.LogTrace(nameof(AuthenticationService), "Found token in cache! \\o/ (Remove: {0})", remove);
            
            if(remove) this._tokenCache.Value = null;
            return this._tokenCache.Value;
        }

        this.Logger.LogTrace(nameof(AuthenticationService), "Did not find token in cache, calling authentication provider.");
        return this._authenticationProvider.AuthenticateToken(context, database);
    }
}