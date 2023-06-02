namespace Bunkum.HttpServer.Database.Dummy;

public class DummyDatabaseProvider : IDatabaseProvider<DummyDatabaseContext>
{
    public void Initialize()
    {
        
    }

    public void Warmup()
    {
        
    }

    public DummyDatabaseContext GetContext()
    {
        return new DummyDatabaseContext();
    }
    
    public void Dispose() {}
}