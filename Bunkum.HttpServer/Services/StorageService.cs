using System.Reflection;
using Bunkum.CustomHttpListener.Request;
using Bunkum.HttpServer.Database;
using Bunkum.HttpServer.Storage;
using NotEnoughLogs;

namespace Bunkum.HttpServer.Services;

public class StorageService : Service
{
    private readonly IDataStore _dataStore;
    
    internal StorageService(Logger logger, IDataStore dataStore) : base(logger)
    {
        this._dataStore = dataStore;
    }

    public override object? AddParameterToEndpoint(ListenerContext context, ParameterInfo parameter, Lazy<IDatabaseContext> database)
    {
        if(ParameterBasedFrom<IDataStore>(parameter))
            return this._dataStore;

        return null;
    }
}