using BenchmarkDotNet.Attributes;
using Bunkum.Core.Listener.Request;
using Bunkum.Core.RateLimit;
using BunkumTests.HttpServer.Tests.RateLimit;
using BunkumTests.HttpServer.Time;
using JetBrains.Annotations;

namespace BunkumTests.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(invocationCount: 50_000)]
public class RateLimitBenchmarks
{
    private static readonly ListenerContext Ctx = new DirectHttpListenerContext();

    private RateLimiter _rateLimiter = null!;
    private MockTimeProvider _timeProvider = null!;
    private MockRateLimitUser _user = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        this._timeProvider = new MockTimeProvider();
        this._rateLimiter = new RateLimiter(this._timeProvider);
        this._user = new MockRateLimitUser("user");
    }

    [Params(1, 10, 25, 50, 100)]
    [UsedImplicitly]
    public int Requests { get; set; }

    [Benchmark]
    public void RateLimit()
    {
        for (int i = 0; i < this.Requests; i++)
        {
            this._rateLimiter.UserViolatesRateLimit(Ctx, null, this._user);
        }
    }
}