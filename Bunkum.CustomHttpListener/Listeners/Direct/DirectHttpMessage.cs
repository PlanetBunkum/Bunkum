namespace Bunkum.CustomHttpListener.Listeners.Direct;

public record DirectHttpMessage(HttpRequestMessage Message, MemoryStream Stream, ManualResetEventSlim Reset);