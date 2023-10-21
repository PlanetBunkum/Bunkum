using System.Net;

namespace Bunkum.Protocols.Gemini.Responses;

public enum GeminiStatusCode
{
    //Input
    Input = 10,
    SensitiveInput = 11,
    //Success
    Success = 20,
    //Redirect
    TemporaryRedirect = 30,
    PermanentRedirect = 31,
    //Temporary failure
    TemporaryFailure = 40,
    ServerUnavailable = 41,
    CgiError = 42,
    ProxyError = 43,
    Slowdown = 44,
    //Permanent failure
    PermanentFailure = 50,
    NotFound = 51,
    Gone = 52,
    ProxyRequestRefused = 53,
    BadRequest = 59,
    //Client certificate required
    ClientCertificateRequired = 60,
    CertificateNotAuthorized = 61,
    CertificateNotValid = 62,
}

internal static class HttpStatusCodeExtensions
{
    public static GeminiStatusCode ToGemini(this HttpStatusCode code)
    {
        return code switch
        {
            HttpStatusCode.OK => GeminiStatusCode.Success,
            HttpStatusCode.Accepted => GeminiStatusCode.Success,
            HttpStatusCode.NoContent => GeminiStatusCode.Success,
            HttpStatusCode.PartialContent => GeminiStatusCode.Success,
            HttpStatusCode.Moved => GeminiStatusCode.PermanentRedirect,
            HttpStatusCode.Found => GeminiStatusCode.TemporaryRedirect,
            HttpStatusCode.Unused => (GeminiStatusCode)69, // :)
            HttpStatusCode.RedirectKeepVerb => GeminiStatusCode.TemporaryRedirect,
            HttpStatusCode.PermanentRedirect => GeminiStatusCode.PermanentRedirect,
            HttpStatusCode.BadRequest => GeminiStatusCode.BadRequest,
            HttpStatusCode.Unauthorized => GeminiStatusCode.CertificateNotAuthorized,
            HttpStatusCode.Forbidden => GeminiStatusCode.CertificateNotAuthorized,
            HttpStatusCode.NotFound => GeminiStatusCode.NotFound,
            HttpStatusCode.ProxyAuthenticationRequired => GeminiStatusCode.ProxyRequestRefused,
            HttpStatusCode.RequestTimeout => GeminiStatusCode.TemporaryFailure,
            HttpStatusCode.Conflict => GeminiStatusCode.TemporaryFailure,
            HttpStatusCode.Gone => GeminiStatusCode.Gone,
            HttpStatusCode.Locked => GeminiStatusCode.CertificateNotAuthorized,
            HttpStatusCode.UpgradeRequired => GeminiStatusCode.ClientCertificateRequired,
            HttpStatusCode.TooManyRequests => GeminiStatusCode.Slowdown,
            HttpStatusCode.UnavailableForLegalReasons => GeminiStatusCode.Gone,
            HttpStatusCode.InternalServerError => GeminiStatusCode.CgiError,
            HttpStatusCode.NotImplemented => GeminiStatusCode.TemporaryFailure,
            HttpStatusCode.BadGateway => GeminiStatusCode.ProxyError,
            HttpStatusCode.ServiceUnavailable => GeminiStatusCode.ServerUnavailable,
            HttpStatusCode.GatewayTimeout => GeminiStatusCode.ServerUnavailable,
            HttpStatusCode.InsufficientStorage => GeminiStatusCode.TemporaryFailure,
            HttpStatusCode.NetworkAuthenticationRequired => GeminiStatusCode.ClientCertificateRequired,
            HttpStatusCode.Continue => GeminiStatusCode.Input,
            HttpStatusCode.Created => GeminiStatusCode.Success,
            HttpStatusCode.PaymentRequired => GeminiStatusCode.SensitiveInput,
            HttpStatusCode.NotAcceptable => GeminiStatusCode.CertificateNotValid,
            HttpStatusCode.ExpectationFailed => GeminiStatusCode.SensitiveInput,
            HttpStatusCode.HttpVersionNotSupported => GeminiStatusCode.PermanentFailure,
            _ => (GeminiStatusCode)code,
        };
    }
}