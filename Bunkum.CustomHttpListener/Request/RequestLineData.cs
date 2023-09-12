namespace Bunkum.CustomHttpListener.Request;

internal ref struct RequestLineData
{
    internal Span<char> MethodBytes;
    internal Span<char> PathBytes;
    internal Span<char> VersionBytes;

    internal RequestLineData(Span<char> methodBytes, Span<char> pathBytes, Span<char> versionBytes)
    {
        this.MethodBytes = methodBytes;
        this.PathBytes = pathBytes;
        this.VersionBytes = versionBytes;
    }
}