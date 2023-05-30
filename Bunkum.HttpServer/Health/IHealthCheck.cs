namespace Bunkum.HttpServer.Health;

public interface IHealthCheck
{
    public string Name { get; }
    public HealthStatus RunCheck();
}