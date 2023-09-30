namespace Bunkum.Protocols.Http.Direct;

public record DirectHttpMessage(HttpRequestMessage Message, MemoryStream Stream, ManualResetEventSlim Reset);