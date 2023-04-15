using Bunkum.HttpServer.Time;

namespace BunkumTests.HttpServer.Time;

public class MockTimeProvider : ITimeProvider
{
    public int Seconds { get; set; }
}