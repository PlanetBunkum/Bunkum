using Bunkum.HttpServer.Database;

namespace BunkumTests.HttpServer.Database;

public class TestSwitchDatabaseContext : IDatabaseContext
{
    public int GetDummyValue() => 420;

    public void Dispose() { }
}