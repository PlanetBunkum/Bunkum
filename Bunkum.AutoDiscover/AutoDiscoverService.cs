using System.Reflection;
using Bunkum.CustomHttpListener.Request;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Database;
using Bunkum.HttpServer.Services;
using NotEnoughLogs;

namespace Bunkum.AutoDiscover;

public class AutoDiscoverService : Service
{
    private readonly AutoDiscoverConfig _config;

    public AutoDiscoverService(LoggerContainer<BunkumContext> logger, AutoDiscoverConfig config) : base(logger)
    {
        this._config = config;
    }

    public override object? AddParameterToEndpoint(ListenerContext context, ParameterInfo paramInfo, Lazy<IDatabaseContext> database)
    {
        if (paramInfo.ParameterType == typeof(AutoDiscoverConfig))
        {
            return _config;
        }

        return base.AddParameterToEndpoint(context, paramInfo, database);
    }
}