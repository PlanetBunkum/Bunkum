# Bunkum.Protocols.Gemini

Gemini protocol support for Bunkum

## Usage

```csharp
using Bunkum.Core;
using Bunkum.Protocols.Gemini;

BunkumServer server = new BunkumGeminiServer(null, new LoggerConfiguration
{
    Behaviour = new QueueLoggingBehaviour(),
    #if DEBUG
    MaxLevel = LogLevel.Trace,
    #else
    MaxLevel = LogLevel.Info,
    #endif
});

server.Initialize = s =>
{
    s.DiscoverEndpointsFromAssembly(Assembly.GetExecutingAssembly());
};

server.Start();
await Task.Delay(-1);
```

## Generating an SSL/TLS certificate

Gemini does not support unencrypted transport by design.
To test locally, you must create a certificate.
Bunkum cannot create one for you, but common tools like OpenSSL can do this for you.

### Linux

First, create a private key/certificate pair.
Change "localhost" to your FQDN if you'd like to use a self-signed certificate externally.

```sh
openssl req -x509 -newkey rsa:4096 -keyout key.pem -out cert.pem -days 365 -nodes -subj "/CN=localhost"
```

Then, convert your pair to a .pfx file that Bunkum can use:

```sh
openssl pkcs12 -export -out cert.pfx -inkey key.pem -in cert.pem
```

Hopefully, you should now have a file named `cert.pfx` that Bunkum can use.

### Windows

TODO

## Configuration

The `BunkumGeminiServer` allows a SSL configuration in the constructor.
If one is not provided, it will default to creating a json config named `geminissl.json` instead:

```json
{
  "Version": 1,
  "SslCertificate": "cert.pfx",
  "CertificatePassword": "password here or null"
}
```

`Version` refers to the configuration schema version, do not modify this.

`SslCertificate` points to the path of your certificate.

`CertificatePassword`  can either be the password as a string or simply `null` in the case of no password. 