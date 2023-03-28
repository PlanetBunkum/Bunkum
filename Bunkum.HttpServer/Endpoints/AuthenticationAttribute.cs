using Bunkum.HttpServer.Authentication;

namespace Bunkum.HttpServer.Endpoints;

/// <summary>
/// Is authentication required for this endpoint?
/// If true, clients will receive 403 if your <see cref="IAuthenticationProvider{TUser}"/> does not return a user.
/// If false, endpoints will work as normal.
/// <br/>
/// Default depends on the value set for <see cref="BunkumHttpServer.AssumeAuthenticationRequired"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class AuthenticationAttribute : Attribute
{
    public readonly bool Required;

    public AuthenticationAttribute(bool required)
    {
        this.Required = required;
    }
}