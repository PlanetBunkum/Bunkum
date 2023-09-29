namespace Bunkum.Core.Health;

public struct HealthStatus
{
    public string CheckName = string.Empty;
    public readonly HealthStatusType StatusType;
    public readonly string Description;

    public HealthStatus(HealthStatusType statusType, string description = "")
    {
        this.StatusType = statusType;
        this.Description = description;
    }

    public HealthStatus(HealthStatusType statusType, Exception exception)
    {
        this.StatusType = statusType;
        this.Description = $"{exception.GetType().Name} {exception.HResult}: {exception.Message}";
    }
}