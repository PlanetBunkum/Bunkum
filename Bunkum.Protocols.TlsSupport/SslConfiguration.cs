using System.Net.Security;
using System.Security.Authentication;
using Bunkum.Core.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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

    /// <summary>
    /// The SSL protocols which are enabled. If null, enables TLS1.3 and TLS1.2
    /// </summary>
    [JsonProperty("EnabledSslProtocols", ItemConverterType = typeof(StringEnumConverter))]
    private SslProtocols[]? _EnabledSslProtocols { get; set; }
    /// <summary>
    /// The cipher suites which are enabled. If null, lets the system decide
    /// </summary>
    [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
    public TlsCipherSuite[]? EnabledCipherSuites { get; set; }

    [JsonIgnore]
    public SslProtocols EnabledSslProtocols
    {
        get
        {
            SslProtocols protocols = SslProtocols.None;
            
            if (this._EnabledSslProtocols == null)
                protocols = SslProtocols.Tls12 | SslProtocols.Tls13;
            else
                protocols = this._EnabledSslProtocols
                    .Aggregate(protocols, (current, protocol) => current | protocol);

            return protocols;
        }
    }
}