using System.Collections.Concurrent;
using System.Diagnostics;
using Bunkum.CustomHttpListener.Listeners.Direct;
using Bunkum.HttpServer;
using JetBrains.Annotations;

namespace BunkumTests.HttpServer;

[Parallelizable]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class ServerDependentTest
{
    private readonly ConcurrentQueue<Action> _stopTasks = new();

    [Pure]
    private protected (BunkumHttpServer, HttpClient) Setup(bool start = true)
    {
        DirectHttpListener listener = new();
        HttpClient client = listener.GetClient();

        BunkumHttpServer server = new(listener);
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