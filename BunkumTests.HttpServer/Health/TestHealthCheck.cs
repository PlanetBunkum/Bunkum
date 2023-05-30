using Bunkum.HttpServer.Health;

namespace BunkumTests.HttpServer.Health;

public class TestHealthCheck : IHealthCheck
{
    private readonly HealthStatusType _type;
    public TestHealthCheck(HealthStatusType type)
    {
        this._type = type;
        this.Name = type.ToString();
    }
    
    public string Name { get; }
    public HealthStatus RunCheck() => new(this._type);
}