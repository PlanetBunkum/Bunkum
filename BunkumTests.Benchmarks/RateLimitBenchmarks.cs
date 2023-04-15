using BenchmarkDotNet.Attributes;
using Bunkum.HttpServer;
using Bunkum.HttpServer.RateLimit;
using BunkumTests.HttpServer.Tests.RateLimit;
using BunkumTests.HttpServer.Time;

namespace BunkumTests.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(invocationCount: 100)]
public class RateLimitBenchmarks
{
    private static readonly RequestContext Ctx = new();
    private RateLimiter _rateLimiter = null!;
    private MockTimeProvider _timeProvider = null!;

    [IterationSetup]
    public void SetUp()
    {
        this._timeProvider = new MockTimeProvider();
        this._rateLimiter = new RateLimiter(this._timeProvider);
    }

    [Params(1, 2, 50, 100)]
    public int Users { get; set; }
    
    [Params(1, 25, 50)]
    public int Requests { get; set; }

    [Benchmark]
    public void RateLimit()
    {
        // Technically shouldn't be allocating users here but i'm not sure how else to test
        for (int i = 0; i < this.Users; i++)
        {
            MockRateLimitUser user = new(i.ToString());
            for (int j = 0; j < this.Requests; j++)
            {
                this._rateLimiter.ViolatesRateLimit(Ctx, user);
            }
        }
    }
}