using Bunkum.HttpServer;
using Bunkum.HttpServer.Database.Dummy;
using Bunkum.HttpServer.Endpoints;
using BunkumTests.HttpServer.Database;

namespace BunkumTests.HttpServer.Endpoints;

public class DatabaseEndpoints : EndpointGroup
{
    [Endpoint("/db/null")]
    public string DatabaseNull(RequestContext context, DummyDatabaseContext? database)
    {
        return (database != null).ToString();
    }
    
    [Endpoint("/db/value")]
    public string DatabaseValue(RequestContext context, DummyDatabaseContext database)
    {
        return database.GetDummyValue().ToString();
    }
    
    [Endpoint("/db/switch")]
    public string DatabaseValue(RequestContext context, TestSwitchDatabaseContext database)
    {
        return database.GetDummyValue().ToString();
    }
}