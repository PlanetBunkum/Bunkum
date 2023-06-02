namespace Bunkum.HttpServer.Database;

public interface IDatabaseProvider<out TDatabaseContext> : IDisposable where TDatabaseContext : IDatabaseContext
{
    void Initialize();

    void Warmup();

    TDatabaseContext GetContext();
}