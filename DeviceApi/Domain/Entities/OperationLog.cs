namespace DeviceApi.Domain.Entities;

public class OperationLog
{
    public Guid Id { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;

    public string ResourceType { get; set; } = string.Empty;

    public string ResourceId { get; set; } = string.Empty;

    public string Detail { get; set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; set; }
}
