namespace Bunkum.Core.Listener.Listeners.Direct;

public record DirectHttpMessage(HttpRequestMessage Message, MemoryStream Stream, ManualResetEventSlim Reset);