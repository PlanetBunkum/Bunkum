using System.Diagnostics.CodeAnalysis;

namespace Bunkum.Core.Configuration;

/// <summary>
/// Bunkum's configuration file. Contains common settings.
/// </summary>
// disable r# warnings about defaults - verbosity is preferred over cleanliness here
// someone might be reading this source to find the default values
[SuppressMessage("ReSharper", "RedundantDefaultMemberInitializer")]
public class BunkumConfig : Config
{
    public override int CurrentConfigVersion => 4;
    public override int Version { get; set; } = 0;
    protected internal override void Migrate(int oldVer, dynamic oldConfig)
    {
        
    }
    
    public string ExternalUrl { get; set; } = "http://127.0.0.1:10061";
    public string ListenHost { get; set; } = "0.0.0.0";
    public int ListenPort { get; set; } = 10061;
    public int ThreadCount { get; set; } = 4;
    public bool UseForwardedIp { get; set; } = false;
}