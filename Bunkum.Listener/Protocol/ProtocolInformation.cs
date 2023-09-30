using Bunkum.Listener.Request;

namespace Bunkum.Listener.Protocol;

/// <summary>
/// A set of information that describes the protocol being used by a request.
/// </summary>
/// <remarks>
/// It is recommended to store this object statically somewhere and then reference it when generating the <see cref="ListenerContext"/> if possible.
/// See <c>HttpProtocolInformation</c> of <c>Bunkum.Protocols.Http</c> for an example.
/// </remarks>
public readonly struct ProtocolInformation
{
    /// <summary>
    /// Instantiates a <see cref="ProtocolInformation"/>.
    /// </summary>
    /// <param name="name">The friendly name of the protocol being used. For example, HTTP would simply be <c>HTTP</c>.</param>
    /// <param name="version">The version of the protocol being used. For example, HTTP/1.1 would simply be <c>1.1</c>.</param>
    public ProtocolInformation(string name, string version)
    {
        this.Name = name;
        this.Version = version;
    }

    /// <summary>
    /// The friendly name of the protocol being used. For example, HTTP would simply be <c>HTTP</c>.
    /// </summary>
    public readonly string Name;
    
    /// <summary>
    /// The version of the protocol being used. For example, HTTP/1.1 would simply be <c>1.1</c>.
    /// </summary>
    public readonly string Version;
}