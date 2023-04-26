using System.Net;
using System.Reflection;
using Bunkum.CustomHttpListener.Request;
using Bunkum.HttpServer.Authentication;
using Bunkum.HttpServer.Database;
using Bunkum.HttpServer.RateLimit;
using Bunkum.HttpServer.Responses;
using NotEnoughLogs;

namespace Bunkum.HttpServer.Services;

public class RateLimitService : Service
{
    private readonly IRateLimiter _rateLimiter;
    private readonly AuthenticationService _authService;

    internal RateLimitService(LoggerContainer<BunkumContext> logger, AuthenticationService authService)
        : this(logger, authService, new RateLimiter())
    {}
    
    public RateLimitService(LoggerContainer<BunkumContext> logger, AuthenticationService authService, IRateLimiter rateLimiter)
        : base(logger)
    {
        this._rateLimiter = rateLimiter;
        this._authService = authService;
    }

    public override Response? OnRequestHandled(ListenerContext context, MethodInfo method, Lazy<IDatabaseContext> database)
    {
        IUser? user = this._authService.AuthenticateUser(context, database);
            
        if (user is null)
            return null;
        
        if (user is not IRateLimitUser rateLimitUser) 
            throw new InvalidOperationException($"Cannot rate-limit a user that does not extend {nameof(IRateLimitUser)}");

        bool violated = this._rateLimiter.ViolatesRateLimit(context, rateLimitUser);

        if (violated) return HttpStatusCode.TooManyRequests;
        return null;
    }
}