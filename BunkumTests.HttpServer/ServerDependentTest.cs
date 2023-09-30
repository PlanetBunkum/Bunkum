using System.Collections.Concurrent;
using System.Diagnostics;
using Bunkum.Core;
using Bunkum.Listener.Listeners.Direct;
using JetBrains.Annotations;
using NotEnoughLogs;

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

        BunkumServer server = new(httpListener);
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