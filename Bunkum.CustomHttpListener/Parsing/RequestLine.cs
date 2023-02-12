namespace Bunkum.CustomHttpListener.Parsing;

#nullable disable

internal struct RequestLine
{
    internal string Method;
    internal string Path;
    internal string Version;
}