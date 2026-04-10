using DeviceApi.Domain.Enums;

namespace DeviceApi.Domain.Entities;

public class Device
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public DeviceType Type { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }

    public ICollection<DataPoint> DataPoints { get; set; } = new List<DataPoint>();
}
