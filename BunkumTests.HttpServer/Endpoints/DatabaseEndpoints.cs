using Bunkum.Core;
using Bunkum.Core.Database.Dummy;
using Bunkum.Core.Endpoints;
using Bunkum.Protocols.Http;
using BunkumTests.HttpServer.Database;

namespace BunkumTests.HttpServer.Endpoints;

public class DatabaseEndpoints : EndpointGroup
{
    [HttpEndpoint("/db/null")]
    public string DatabaseNull(RequestContext context, DummyDatabaseContext? database)
    {
        return (database != null).ToString();
    }
    
    [HttpEndpoint("/db/value")]
    public string DatabaseValue(RequestContext context, DummyDatabaseContext database)
    {
        return database.GetDummyValue().ToString();
    }
    
    [HttpEndpoint("/db/switch")]
    public string DatabaseValue(RequestContext context, TestSwitchDatabaseContext database)
    {
        return database.GetDummyValue().ToString();
    }
}