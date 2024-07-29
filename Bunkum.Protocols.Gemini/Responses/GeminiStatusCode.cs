using System.Net;

namespace Bunkum.Protocols.Gemini.Responses;

public enum GeminiStatusCode
{
    // Input
    
    /// <summary>
    /// The basic input status code. It should be URI-encoded and sent as a query to the same URI that generated this response.
    /// </summary>
    Input = 10,
    /// <summary>
    /// As per status code 10, but for use with sensitive input such as passwords.
    /// Clients should present the prompt as per status code 10,
    /// but the user's input should not be echoed to the screen to prevent it being read by "shoulder surfers".
    /// </summary>
    SensitiveInput = 11,
    
    //Success
    
    /// <summary>
    /// The server has successfully parsed and understood the request, and will serve up content of the given MIME type.
    /// </summary>
    Success = 20,
    
    //Redirection
    
    /// <summary>
    /// The basic redirection code. The redirection is temporary and the client should continue to request the content with the original URI.
    /// </summary>
    TemporaryRedirection = 30,
    /// <summary>
    /// The location of the content has moved permanently to a new location,
    /// and clients SHOULD use the new location to retrieve the given content from then on.
    /// </summary>
    PermanentRedirection = 31,
    
    //Temporary failure
    
    /// <summary>
    /// An unspecified condition exists on the server that is preventing the content from being served,
    /// but a client can try again to obtain the content.
    /// </summary>
    TemporaryFailure = 40,
    /// <summary>
    /// The server is unavailable due to overload or maintenance. (cf HTTP 503)
    /// </summary>
    ServerUnavailable = 41,
    /// <summary>
    /// A CGI process, or similar system for generating dynamic content, died unexpectedly or timed out.
    /// </summary>
    CgiError = 42,
    /// <summary>
    /// A proxy request failed because the server was unable to successfully complete a transaction with the remote host. (cf HTTP 502, 504)
    /// </summary>
    ProxyError = 43,
    /// <summary>
    /// The server is requesting the client to slow down requests.
    /// </summary>
    Slowdown = 44,
    
    //Permanent failure
    
    /// <summary>
    /// This is the general permanent failure code.
    /// </summary>
    PermanentFailure = 50,
    /// <summary>
    /// The requested resource could not be found and no further information is available. It may exist in the future, it may not.
    /// </summary>
    NotFound = 51,
    /// <summary>
    /// The resource requested is no longer available and will not be available again.
    /// Search engines and similar tools should remove this resource from their indices.
    /// Content aggregators should stop requesting the resource and convey to their human users that the subscribed resource is gone. (cf HTTP 410)
    /// </summary>
    Gone = 52,
    /// <summary>
    /// The request was for a resource at a domain not served by the server and the server does not accept proxy requests.
    /// </summary>
    ProxyRequestRefused = 53,
    /// <summary>
    /// The server was unable to parse the client's request, presumably due to a malformed request.
    /// </summary>
    BadRequest = 59,
    
    //Client certificate required
    
    /// <summary>
    /// The content requires a client certificate.
    /// </summary>
    ClientCertificateRequired = 60,
    /// <summary>
    /// The supplied client certificate is not authorised for accessing the particular requested resource.
    /// The problem is not with the certificate itself, which may be authorised for other resources.
    /// </summary>
    CertificateNotAuthorized = 61,
    /// <summary>
    /// The supplied client certificate was not accepted because it is not valid.
    /// This indicates a problem with the certificate in and of itself,
    /// with no consideration of the particular requested resource.
    /// 
    /// The most likely cause is that the certificate's validity start date is in the future or its expiry date has passed,
    /// but this code may also indicate an invalid signature, or a violation of a X509 standard requirements.
    /// </summary>
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
            HttpStatusCode.Moved => GeminiStatusCode.PermanentRedirection,
            HttpStatusCode.Found => GeminiStatusCode.TemporaryRedirection,
            HttpStatusCode.Unused => (GeminiStatusCode)69, // :)
            HttpStatusCode.RedirectKeepVerb => GeminiStatusCode.TemporaryRedirection,
            HttpStatusCode.PermanentRedirect => GeminiStatusCode.PermanentRedirection,
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