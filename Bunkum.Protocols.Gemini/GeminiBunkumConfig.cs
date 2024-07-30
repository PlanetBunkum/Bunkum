using Bunkum.Core.Configuration;

namespace Bunkum.Protocols.Gemini;

/// <summary>
/// An extended form of BunkumConfig with extra configuration options
/// </summary>
public class GeminiBunkumConfig : BunkumConfig
{
    /// <summary>
    /// Whether or not to allow proxy requests
    /// </summary>
    public bool AllowProxyRequests { get; set; } = false;

    public override int CurrentConfigVersion => base.CurrentConfigVersion + 1;
}