namespace Bunkum.Core.Listener.Parsing;

public enum HttpMethod
{
    Invalid,
    Get,
    Put,
    Post,
    Delete,
    Head,
    Options,
    Trace,
    Patch,
    Connect,
}

public static class MethodUtils
{
    public static HttpMethod FromString(ReadOnlySpan<char> str)
    {
        // ReSharper disable once LoopCanBeConvertedToQuery (this literally won't compile)
        foreach (HttpMethod m in Enum.GetValues<HttpMethod>())
        {
            if (m.ToString().ToUpperInvariant().AsSpan().SequenceEqual(str)) return m;
        }

        return HttpMethod.Invalid;
    }
}