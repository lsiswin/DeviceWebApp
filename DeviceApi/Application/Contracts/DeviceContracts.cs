using DeviceApi.Domain.Entities;
using DeviceApi.Domain.Enums;

namespace DeviceApi.Application.Contracts;

public sealed record DeviceDto(
    Guid Id,
    string Name,
    DeviceType Type,
    ProtocolType ProtocolType,
    DeviceStatus Status,
    string ConnectionString,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    IReadOnlyCollection<DataPointDto> DataPoints
);

public sealed record DataPointDto(
    Guid Id,
    Guid DeviceId,
    string Address,
    string Name,
    string DataType,
    string? NodeId,
    int? NamespaceIndex,
    double? AlarmThreshold,
    DateTimeOffset UpdatedAtUtc
);

public sealed record CreateDeviceRequest(
    string Name,
    DeviceType Type,
    ProtocolType ProtocolType,
    DeviceStatus Status,
    string ConnectionString
);

public sealed record UpdateDeviceRequest(
    string Name,
    DeviceType Type,
    ProtocolType ProtocolType,
    DeviceStatus Status,
    string ConnectionString
);

public sealed record CreateDataPointRequest(
    string Address,
    string Name,
    string DataType,
    string? NodeId = null,
    int? NamespaceIndex = null,
    double? AlarmThreshold = null
);

public sealed record UpdateDataPointRequest(
    string Address,
    string Name,
    string DataType,
    string? NodeId = null,
    int? NamespaceIndex = null,
    double? AlarmThreshold = null
);

public sealed record DashboardStatsResponse(
    int DeviceCount,
    int SensorCount,
    int PlcCount,
    int DataPointCount,
    int OnlineDeviceCount,
    int ReportedOnlineDevices = 0,
    int ReportedFaultDevices = 0,
    int ReportedTotalPoints = 0,
    bool IsOpcEngineRunning = false
);

public sealed record OpcServerStatusReport(
    string ServerName,
    int TotalDevices,
    int OnlineDevices,
    int FaultDevices,
    int TotalPoints,
    bool IsEngineRunning,
    DateTimeOffset Timestamp
);

public sealed record OperationLogDto(
    Guid Id,
    string UserName,
    string Action,
    string ResourceType,
    string ResourceId,
    string Detail,
    DateTimeOffset CreatedAtUtc
);

public sealed record PermissionSummaryResponse(
    string UserName,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions
);
