namespace Bunkum.HealthChecks;

public interface IHealthCheck
{
    public string Name { get; }
    public HealthStatus RunCheck();
}