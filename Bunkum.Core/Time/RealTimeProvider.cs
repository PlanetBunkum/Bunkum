namespace Bunkum.Core.Time;

internal class RealTimeProvider : ITimeProvider
{
    public int Seconds => (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
}