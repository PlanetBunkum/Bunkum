using Bunkum.Core.Configuration;

namespace Bunkum.Protocols.Https;

public class SslConfiguration : Config
{
    public override int CurrentConfigVersion => 1;
    public override int Version { get; set; }
    
    protected override void Migrate(int oldVer, dynamic oldConfig)
    {}

    public bool SslEnabled { get; set; } = false;
    public string SslCertificate { get; set; } = "cert.pfx";
    public string? CertificatePassword { get; set; } = "password here or null";
}