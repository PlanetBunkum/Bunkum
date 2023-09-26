using Bunkum.HttpServer.Health;
using BunkumTests.HttpServer.Health;
using NotEnoughLogs;
using static Bunkum.HttpServer.Health.HealthStatusType;

namespace BunkumTests.HttpServer.Tests;

public class HealthCheckTests
{
    [Test]
    [TestCase(Unhealthy)]
    [TestCase(Degraded)]
    [TestCase(Healthy)]
    public void HealthChecksWork(HealthStatusType type)
    {
        TestHealthCheck check = new(type);
        Logger logger = new();
        HealthService healthService = new(logger, new []{check});

        HealthReport report = healthService.GenerateReport();
        Assert.That(report.StatusType, Is.EqualTo(type));
    }
    
    [Test]
    public void PicksCorrectStatus()
    {
        TestHealthCheck check1 = new(Healthy);
        TestHealthCheck check2 = new(Healthy);
        TestHealthCheck check3 = new(Unhealthy);
        
        Logger logger = new();
        HealthService healthService = new(logger, new []{check1, check2, check3});

        HealthReport report = healthService.GenerateReport();
        Assert.That(report.StatusType, Is.EqualTo(Unhealthy));
    }
}