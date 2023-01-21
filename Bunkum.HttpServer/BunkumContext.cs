namespace Bunkum.HttpServer;

public enum BunkumContext
{
    Startup,
    Request,
    Authentication,
    UserContent,
    Filter,
    Configuration,
}