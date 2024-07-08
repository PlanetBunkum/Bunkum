using System.Security.Cryptography.X509Certificates;
using Bunkum.Core;
using Bunkum.Core.Configuration;
using Bunkum.Listener;
using NotEnoughLogs;
using NotEnoughLogs.Sinks;

namespace Bunkum.Protocols.Http;

public class BunkumHttpServer : BunkumServer
{
    private readonly X509Certificate2? _cert;

    public BunkumHttpServer(LoggerConfiguration? configuration = null, List<ILoggerSink>? sinks = null,
        SslConfiguration? sslConfiguration = null) : base(configuration, sinks)
    {
        //If the SSL configuration is not specified, load the config from JSON
        sslConfiguration ??= Config.LoadFromJsonFile<SslConfiguration>("httpssl.json", this.Logger);
        
        this._cert = sslConfiguration.SslEnabled 
            ? new X509Certificate2(File.ReadAllBytes(sslConfiguration.SslCertificate), sslConfiguration.CertificatePassword) 
            : null;
    }

    [Obsolete("This constructor is obsolete, `UseListener` is preferred instead!")]
    public BunkumHttpServer(BunkumListener listener, LoggerConfiguration? configuration = null, List<ILoggerSink>? sinks = null) : base(listener, configuration, sinks)
    {}

    /// <inherit-doc/>
    protected override BunkumListener CreateDefaultListener(Uri listenEndpoint, bool useForwardedIp, Logger logger)
    {
        return new Socket.SocketHttpListener(listenEndpoint, useForwardedIp, logger, this._cert);
    }

    /// <inherit-doc/>
    protected override string ProtocolUriName => "http";
}