using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Bunkum.HealthChecks;

/// <summary>
/// An indicator indicating the status of a service.
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum HealthStatusType : byte
{
    /// <summary>
    /// The service is completely unavailable.
    /// </summary>
    Unhealthy = 0,
    /// <summary>
    /// The service is experiencing a minor interruption, but general operation is still working.
    /// </summary>
    Degraded = 1,
    /// <summary>
    /// The service is up and running with no issues whatsoever.
    /// </summary>
    Healthy = 2,
}