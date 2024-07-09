using System.Security.Cryptography.X509Certificates;
using Bunkum.Core;
using Bunkum.Core.Configuration;
using Bunkum.Listener;
using NotEnoughLogs;
using NotEnoughLogs.Sinks;

namespace Bunkum.Protocols.Https;

public class BunkumHttpsServer : BunkumServer
{
    private readonly X509Certificate2? _cert;

    public BunkumHttpsServer(LoggerConfiguration? configuration = null, List<ILoggerSink>? sinks = null,
        SslConfiguration? sslConfiguration = null) : base(configuration, sinks)
    {
        //If the SSL configuration is not specified, load the config from JSON
        sslConfiguration ??= Config.LoadFromJsonFile<SslConfiguration>("ssl.json", this.Logger);
        
        this._cert = new X509Certificate2(File.ReadAllBytes(sslConfiguration.SslCertificate), sslConfiguration.CertificatePassword);
    }

    /// <inherit-doc/>
    protected override BunkumListener CreateDefaultListener(Uri listenEndpoint, bool useForwardedIp, Logger logger)
    {
        return new Http.Socket.SocketHttpListener(listenEndpoint, useForwardedIp, logger, this._cert);
    }

    /// <inherit-doc/>
    protected override string ProtocolUriName => "https";
}