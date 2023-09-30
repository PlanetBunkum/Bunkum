using BenchmarkDotNet.Attributes;
using Bunkum.Core;
using Bunkum.Core.Listener.Listeners.Direct;
using BunkumTests.HttpServer.Endpoints;
using NotEnoughLogs;

namespace BunkumTests.Benchmarks;

[MemoryDiagnoser]
public class TestRequestBenchmarks
{
    private static readonly Uri Endpoint = new("/");
    
    private DirectHttpListener _httpListener = null!;
    private HttpClient _client = null!;
    private BunkumServer _server = null!;

    [GlobalSetup]
    public void Setup()
    {
        this._httpListener = new DirectHttpListener(new Logger());
        this._client = this._httpListener.GetClient();
        this._server = new BunkumServer(this._httpListener, false);
        
        this._server.DiscoverEndpointsFromAssembly(typeof(TestEndpoints).Assembly);
        this._server.Start(1);
    }

    [Benchmark]
    public Task GetTestEndpoint()
    {
        return this._client.GetAsync(Endpoint);
    }
}