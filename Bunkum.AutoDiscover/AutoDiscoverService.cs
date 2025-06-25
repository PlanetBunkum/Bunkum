using System.Reflection;
using Bunkum.Core;
using Bunkum.Core.Database;
using Bunkum.Core.Services;
using Bunkum.Listener.Request;
using NotEnoughLogs;

namespace Bunkum.AutoDiscover;

/// <summary>
/// An implementation of AutoDiscover for Bunkum.
/// See documentation here on this API here: https://docs.littlebigrefresh.com/autodiscover-api
/// </summary>
public class AutoDiscoverService : Service
{
    private readonly AutoDiscoverConfig _config;

    internal AutoDiscoverService(Logger logger, AutoDiscoverConfig config) : base(logger)
    {
        this._config = config;
    }

    /// <inheritdoc/>
    public override object? AddParameterToEndpoint(ListenerContext context, BunkumParameterInfo parameter, Lazy<IDatabaseContext> database)
    {
        if (parameter.ParameterType == typeof(AutoDiscoverConfig))
        {
            return _config;
        }

        return base.AddParameterToEndpoint(context, parameter, database);
    }
}