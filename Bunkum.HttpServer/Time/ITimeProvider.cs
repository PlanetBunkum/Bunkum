namespace Bunkum.HttpServer.Time;

#if NET8_0
#error Switch to .NET 8 time providers 
#endif

public interface ITimeProvider
{
    public int Seconds { get; }
}
