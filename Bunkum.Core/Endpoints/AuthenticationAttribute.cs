using Bunkum.Core.Authentication;

namespace Bunkum.Core.Endpoints;

/// <summary>
/// Is authentication required for this endpoint?
/// If true, clients will receive 403 if your <see cref="IAuthenticationProvider{TToken}"/> does not return a user.
/// If false, endpoints will work as normal.
/// <br/>
/// Default depends on the value set for <see cref="BunkumServer.AssumeAuthenticationRequired"/>.
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