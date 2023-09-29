namespace Bunkum.Core.Health;

public interface IHealthCheck
{
    public string Name { get; }
    public HealthStatus RunCheck();
}