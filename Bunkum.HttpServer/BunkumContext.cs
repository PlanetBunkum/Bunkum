using System.Diagnostics.CodeAnalysis;

namespace Bunkum.HttpServer;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public enum BunkumContext
{
    Startup,
    Request,
    Authentication,
    UserContent,
    Filter,
    Configuration,
    Digest,
}