using System.Reflection;
using Bunkum.Core.Database;
using Bunkum.Core.Storage;
using Bunkum.Listener.Request;
using NotEnoughLogs;

namespace Bunkum.Core.Services;

public class StorageService : Service
{
    private readonly IDataStore _dataStore;
    
    internal StorageService(Logger logger, IDataStore dataStore) : base(logger)
    {
        this._dataStore = dataStore;
    }
    
    public override object? AddParameterToEndpoint(ListenerContext context, BunkumParameterInfo parameter, Lazy<IDatabaseContext> database)
    {
        if(ParameterBasedFrom<IDataStore>(parameter))
            return this._dataStore;

        return null;
    }
}