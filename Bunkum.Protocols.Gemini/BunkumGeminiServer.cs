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
    private readonly SslConfiguration _sslConfiguration;
    
    /// <summary>
    /// Create a new BunkumGeminiServer
    /// </summary>
    /// <param name="sslConfiguration">The SSL configuration to use, defaults to `geminissl.json`.</param>
    /// <param name="loggerConfiguration">The logger configuration to use, uses sane defaults.</param>
    /// <param name="sinks">The logger sinks for the logger to use, defaults to ConsoleLogger</param>
    public BunkumGeminiServer(SslConfiguration? sslConfiguration = null, LoggerConfiguration? loggerConfiguration = null, List<ILoggerSink>? sinks = null) : base(loggerConfiguration, sinks)
    {
        //If the SSL configuration is not specified, load the config from JSON
        this._sslConfiguration = sslConfiguration ?? Config.LoadFromJsonFile<SslConfiguration>("geminissl.json", this.Logger);

        this._cert = new X509Certificate2(File.ReadAllBytes(this._sslConfiguration.SslCertificate), this._sslConfiguration.CertificatePassword);
    }

    protected override BunkumConfig CreateConfig(string filename) 
        => Config.LoadFromJsonFile<GeminiBunkumConfig>(filename, this.Logger);

    protected override BunkumListener CreateDefaultListener(Uri listenEndpoint, bool useForwardedIp, Logger logger)
    {
        return new SocketGeminiListener(this._cert, this._sslConfiguration, listenEndpoint, logger, (GeminiBunkumConfig)this.BunkumConfig);
    }
    
    protected override string ProtocolUriName => "gemini";
}