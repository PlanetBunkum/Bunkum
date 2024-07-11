using Bunkum.Core.Configuration;

namespace Bunkum.Protocols.TlsSupport;

public class SslConfiguration : Config
{
    public override int CurrentConfigVersion => 1;
    public override int Version { get; set; }
    
    protected override void Migrate(int oldVer, dynamic oldConfig)
    {}

    /// <summary>
    /// The path to the certificate
    /// </summary>
    public string SslCertificate { get; set; } = "cert.pfx";
    /// <summary>
    /// The password for the certificate, null if none
    /// </summary>
    public string? CertificatePassword { get; set; }
}