namespace DeviceApi.Domain.Entities;

/// <summary>
/// 表示操作日志实体，记录用户对资源的操作信息。
/// </summary>
public class OperationLog
{
    /// <summary>
    /// 日志唯一标识符。
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 操作用户名称。
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 操作类型。
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// 操作的资源类型。
    /// </summary>
    public string ResourceType { get; set; } = string.Empty;

    /// <summary>
    /// 操作的资源标识符。
    /// </summary>
    public string ResourceId { get; set; } = string.Empty;

    /// <summary>
    /// 操作详情。
    /// </summary>
    public string Detail { get; set; } = string.Empty;

    /// <summary>
    /// 日志创建时间（UTC）。
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; set; }
}
