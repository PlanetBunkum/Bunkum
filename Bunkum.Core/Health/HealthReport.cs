using Newtonsoft.Json;

namespace Bunkum.Core.Health;

/// <summary>
/// A report summarizing all health checks.
/// </summary>
[JsonObject(MemberSerialization.OptOut)]
public struct HealthReport
{
    public HealthStatusType StatusType;

    public List<HealthStatus> Checks;
}