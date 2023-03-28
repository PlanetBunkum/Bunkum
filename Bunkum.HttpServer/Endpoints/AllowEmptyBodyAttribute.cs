namespace Bunkum.HttpServer.Endpoints;

/// <summary>
/// Allows the endpoint to receive null as a body instead of clients being sent BadRequest.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class AllowEmptyBodyAttribute : Attribute
{}