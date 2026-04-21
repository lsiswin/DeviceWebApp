using DeviceApi.Domain.Enums;

namespace DeviceApi.Domain.Entities;

public class Device
{
    public Guid Id { get; set; }

    public required string Name { get; set; } = string.Empty;

    public DeviceType Type { get; set; }

    public ProtocolType ProtocolType { get; set; }

    public required string ConnectionString { get; set; }

    public DeviceStatus Status { get; set; } = DeviceStatus.Offline;

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }

    public ICollection<DataPoint> DataPoints { get; set; } = new List<DataPoint>();
}

public enum DeviceStatus
{
    Offline = 0,
    Online = 1,
    Fault = 2,
    Connecting = 3,
}
