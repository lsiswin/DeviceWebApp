using DeviceApi.Domain.Enums;

namespace DeviceApi.Application.Contracts;

public sealed record DeviceDto(
    Guid Id,
    string Name,
    DeviceType Type,
    string Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    IReadOnlyCollection<DataPointDto> DataPoints
);

public sealed record DataPointDto(
    Guid Id,
    Guid DeviceId,
    string Key,
    string Name,
    string DataType,
    string Value,
    DateTimeOffset UpdatedAtUtc
);

public sealed record CreateDeviceRequest(string Name, DeviceType Type, string Status);

public sealed record UpdateDeviceRequest(string Name, DeviceType Type, string Status);

public sealed record CreateDataPointRequest(string Key, string Name, string DataType, string Value);

public sealed record UpdateDataPointRequest(string Key, string Name, string DataType, string Value);

public sealed record UpdateDataPointValueRequest(string Value);

public sealed record DashboardStatsResponse(
    int DeviceCount,
    int SensorCount,
    int PlcCount,
    int DataPointCount,
    int OnlineDeviceCount
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
