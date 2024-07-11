using System.Security.Cryptography.X509Certificates;
using Bunkum.Core;
using Bunkum.Core.Configuration;
using Bunkum.Listener;
using Bunkum.Protocols.Gemini.Socket;
using Bunkum.Protocols.TlsSupport;
using NotEnoughLogs;
using NotEnoughLogs.Sinks;

namespace Bunkum.Protocols.Gemini;

public class BunkumGeminiServer : BunkumServer
{
    private readonly X509Certificate2 _cert;
    
    /// <summary>
    /// Create a new BunkumGeminiServer
    /// </summary>
    /// <param name="sslConfiguration">The SSL configuration to use, defaults to `geminissl.json`.</param>
    /// <param name="configuration">The logger configuration to use, uses sane defaults.</param>
    /// <param name="sinks">The logger sinks for the logger to use, defaults to ConsoleLogger</param>
    public BunkumGeminiServer(SslConfiguration? sslConfiguration = null, LoggerConfiguration? configuration = null, List<ILoggerSink>? sinks = null) : base(configuration, sinks)
    {
        //If the SSL configuration is not specified, load the config from JSON
        sslConfiguration ??= Config.LoadFromJsonFile<SslConfiguration>("geminissl.json", this.Logger);

        this._cert = new X509Certificate2(File.ReadAllBytes(sslConfiguration.SslCertificate), sslConfiguration.CertificatePassword);
    }
    
    protected override BunkumListener CreateDefaultListener(Uri listenEndpoint, bool useForwardedIp, Logger logger)
    {
        return new SocketGeminiListener(this._cert, listenEndpoint, logger);
    }
    protected override string ProtocolUriName => "gemini";
}