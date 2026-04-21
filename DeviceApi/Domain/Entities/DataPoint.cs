namespace DeviceApi.Domain.Entities;

public class DataPoint
{
    public Guid Id { get; set; }

    public Guid DeviceId { get; set; }

    public Device? Device { get; set; }

    public string Address { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string DataType { get; set; } = string.Empty;

    /// <summary>
    /// OPC UA NodeId (用于OPC Client订阅数据)
    /// 格式示例: "ns=2;s=Device1.Tag1" 或 "ns=2;i=1001"
    /// </summary>
    public string? NodeId { get; set; }

    /// <summary>
    /// OPC UA Namespace Index (命名空间索引)
    /// 通常 OPC Server 创建节点时会分配 namespace index
    /// </summary>
    public int? NamespaceIndex { get; set; }

    /// <summary>
    /// 自动报警阈值。如果客户端读取的数据转换后超出此值，将触发后台报警日志。
    /// </summary>
    public double? AlarmThreshold { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }
}
