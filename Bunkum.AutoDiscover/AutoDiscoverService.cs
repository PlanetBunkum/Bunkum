using System.Reflection;
using Bunkum.CustomHttpListener.Request;
using Bunkum.HttpServer.Database;
using Bunkum.HttpServer.Services;
using NotEnoughLogs;

namespace Bunkum.AutoDiscover;

/// <summary>
/// An implementation of AutoDiscover for Bunkum.
/// See documentation here on this API here: https://littlebigrefresh.github.io/Docs/autodiscover-api
/// </summary>
public class AutoDiscoverService : Service
{
    private readonly AutoDiscoverConfig _config;

    internal AutoDiscoverService(Logger logger, AutoDiscoverConfig config) : base(logger)
    {
        this._config = config;
    }

    /// <inheritdoc/>
    public override object? AddParameterToEndpoint(ListenerContext context, ParameterInfo parameter, Lazy<IDatabaseContext> database)
    {
        if (parameter.ParameterType == typeof(AutoDiscoverConfig))
        {
            return _config;
        }

        return base.AddParameterToEndpoint(context, parameter, database);
    }
}