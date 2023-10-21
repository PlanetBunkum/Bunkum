using System.Collections.Concurrent;
using System.Diagnostics;
using Bunkum.Core;
using Bunkum.Protocols.Http;
using Bunkum.Protocols.Http.Direct;
using JetBrains.Annotations;
using NotEnoughLogs;
using NotEnoughLogs.Behaviour;

namespace BunkumTests.HttpServer;

[Parallelizable]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class ServerDependentTest
{
    private readonly ConcurrentQueue<Action> _stopTasks = new();

    [Pure]
    private protected (BunkumServer, HttpClient) Setup(bool start = true)
    {
        DirectHttpListener httpListener = new(new Logger());
        HttpClient client = httpListener.GetClient();

        BunkumServer server = new BunkumHttpServer(configuration: new LoggerConfiguration
        {
            Behaviour = new DirectLoggingBehaviour(),
            MaxLevel = LogLevel.Trace,
        });
        server.UseListener(httpListener);
        
        server.AddAuthenticationService();
        if(start) server.Start(0);
        
        this._stopTasks.Enqueue(() => server.Stop());

        return (server, client);
    }

    [TearDown]
    public void TearDown()
    {
        while (this._stopTasks.TryDequeue(out Action? action))
        {
            Debug.Assert(action != null);
            action.Invoke();
        }
    }
}