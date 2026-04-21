using DeviceApi.Application.Contracts;
using DeviceApi.Application.Interfaces;
using DeviceApi.Domain.Entities;
using DeviceApi.Domain.Enums;
using DeviceApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace DeviceApi.Infrastructure.Services;

public class DeviceService(
    AppDbContext dbContext,
    IMemoryCache memoryCache,
    IOperationLogService operationLogService,
    ILogger<DeviceService> logger
) : IDeviceService
{
    private const string DevicesCacheKey = "device:list";
    private const string DashboardCacheKey = "dashboard:stats";
    private const string OpcStatusCacheKey = "opc:server:status";
    private static readonly TimeSpan CacheLifetime = TimeSpan.FromMinutes(5);

    public async Task ReportOpcStatusAsync(OpcServerStatusReport report, CancellationToken cancellationToken)
    {
        memoryCache.Set(OpcStatusCacheKey, report, TimeSpan.FromMinutes(2));
        memoryCache.Remove(DashboardCacheKey); // 状态更新后清理仪表盘缓存
        await Task.CompletedTask;
    }

    public async Task<IReadOnlyCollection<DeviceDto>> GetDevicesAsync(
        CancellationToken cancellationToken
    )
    {
        if (
            memoryCache.TryGetValue<IReadOnlyCollection<DeviceDto>>(DevicesCacheKey, out var cached)
            && cached is not null
        )
        {
            return cached;
        }

        var devices = await dbContext
            .Devices.AsNoTracking()
            .Include(x => x.DataPoints)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        var result = devices.Select(ToDeviceDto).ToArray();
        memoryCache.Set(DevicesCacheKey, result, CacheLifetime);
        return result;
    }

    public async Task<DeviceDto?> GetDeviceAsync(Guid id, CancellationToken cancellationToken)
    {
        var device = await dbContext
            .Devices.AsNoTracking()
            .Include(x => x.DataPoints)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return device is null ? null : ToDeviceDto(device);
    }

    public async Task<DeviceDto> CreateDeviceAsync(
        CreateDeviceRequest request,
        CancellationToken cancellationToken
    )
    {
        var now = DateTimeOffset.UtcNow;
        var entity = new Device
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Type = request.Type,
            ProtocolType = request.ProtocolType,
            Status = request.Status,
            ConnectionString = request.ConnectionString.Trim(),
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };

        dbContext.Devices.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        InvalidateCache();
        await operationLogService.WriteAsync(
            "CreateDevice",
            "Device",
            entity.Id.ToString(),
            $"创建设备:{entity.Name}",
            cancellationToken
        );
        logger.LogInformation("设备已创建 {DeviceId} - {DeviceName}", entity.Id, entity.Name);

        return ToDeviceDto(entity);
    }

    public async Task<DeviceDto?> UpdateDeviceAsync(
        Guid id,
        UpdateDeviceRequest request,
        CancellationToken cancellationToken
    )
    {
        var device = await dbContext
            .Devices.Include(x => x.DataPoints)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (device is null)
        {
            return null;
        }

        device.Name = request.Name.Trim();
        device.Type = request.Type;
        device.ProtocolType = request.ProtocolType;
        device.Status = request.Status;
        device.ConnectionString = request.ConnectionString.Trim();
        device.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        InvalidateCache();
        await operationLogService.WriteAsync(
            "UpdateDevice",
            "Device",
            id.ToString(),
            $"更新设备:{device.Name}",
            cancellationToken
        );
        logger.LogInformation("设备已更新 {DeviceId}", id);
        return ToDeviceDto(device);
    }

    public async Task<bool> DeleteDeviceAsync(Guid id, CancellationToken cancellationToken)
    {
        var device = await dbContext.Devices.FirstOrDefaultAsync(
            x => x.Id == id,
            cancellationToken
        );
        if (device is null)
        {
            return false;
        }

        dbContext.Devices.Remove(device);
        await dbContext.SaveChangesAsync(cancellationToken);
        InvalidateCache();
        await operationLogService.WriteAsync(
            "DeleteDevice",
            "Device",
            id.ToString(),
            $"删除设备:{device.Name}",
            cancellationToken
        );
        logger.LogInformation("设备已删除 {DeviceId}", id);
        return true;
    }

    public async Task<IReadOnlyCollection<DataPointDto>?> GetDataPointsAsync(
        Guid deviceId,
        CancellationToken cancellationToken
    )
    {
        var exists = await dbContext
            .Devices.AsNoTracking()
            .AnyAsync(x => x.Id == deviceId, cancellationToken);
        if (!exists)
        {
            return null;
        }

        var points = await dbContext
            .DataPoints.AsNoTracking()
            .Where(x => x.DeviceId == deviceId)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return points.Select(ToDataPointDto).ToArray();
    }

    public async Task<DataPointDto?> GetDataPointAsync(
        Guid deviceId,
        Guid pointId,
        CancellationToken cancellationToken
    )
    {
        var point = await dbContext
            .DataPoints.AsNoTracking()
            .FirstOrDefaultAsync(x => x.DeviceId == deviceId && x.Id == pointId, cancellationToken);

        return point is null ? null : ToDataPointDto(point);
    }

    public async Task<DataPointDto?> CreateDataPointAsync(
        Guid deviceId,
        CreateDataPointRequest request,
        CancellationToken cancellationToken
    )
    {
        var exists = await dbContext.Devices.AnyAsync(x => x.Id == deviceId, cancellationToken);
        if (!exists)
        {
            return null;
        }

        var point = new DataPoint
        {
            Id = Guid.NewGuid(),
            DeviceId = deviceId,
            Address = request.Address.Trim(),
            Name = request.Name.Trim(),
            DataType = request.DataType.Trim(),
            NodeId = request.NodeId?.Trim(),
            NamespaceIndex = request.NamespaceIndex,
            AlarmThreshold = request.AlarmThreshold,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
        };

        dbContext.DataPoints.Add(point);
        await dbContext.SaveChangesAsync(cancellationToken);
        InvalidateCache();
        await operationLogService.WriteAsync(
            "CreateDataPoint",
            "DataPoint",
            point.Id.ToString(),
            $"创建数据点:{point.Address}",
            cancellationToken
        );
        logger.LogInformation("数据点已创建: {PointId} on device {DeviceId}", point.Id, deviceId);
        return ToDataPointDto(point);
    }

    public async Task<DataPointDto?> UpdateDataPointAsync(
        Guid deviceId,
        Guid pointId,
        UpdateDataPointRequest request,
        CancellationToken cancellationToken
    )
    {
        var point = await dbContext.DataPoints.FirstOrDefaultAsync(
            x => x.DeviceId == deviceId && x.Id == pointId,
            cancellationToken
        );
        if (point is null)
        {
            return null;
        }

        point.Address = request.Address.Trim();
        point.Name = request.Name.Trim();
        point.DataType = request.DataType.Trim();
        point.NodeId = request.NodeId?.Trim();
        point.NamespaceIndex = request.NamespaceIndex;
        point.AlarmThreshold = request.AlarmThreshold;
        point.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        InvalidateCache();
        await operationLogService.WriteAsync(
            "UpdateDataPoint",
            "DataPoint",
            pointId.ToString(),
            $"更新数据点:{point.Address}",
            cancellationToken
        );
        logger.LogInformation("数据点已更新: {PointId}", pointId);
        return ToDataPointDto(point);
    }

    public async Task<bool> DeleteDataPointAsync(
        Guid deviceId,
        Guid pointId,
        CancellationToken cancellationToken
    )
    {
        var point = await dbContext.DataPoints.FirstOrDefaultAsync(
            x => x.DeviceId == deviceId && x.Id == pointId,
            cancellationToken
        );
        if (point is null)
        {
            return false;
        }

        dbContext.DataPoints.Remove(point);
        await dbContext.SaveChangesAsync(cancellationToken);
        InvalidateCache();
        await operationLogService.WriteAsync(
            "DeleteDataPoint",
            "DataPoint",
            pointId.ToString(),
            $"删除数据点:{point.Address}",
            cancellationToken
        );
        logger.LogInformation("数据点已删除: {PointId}", pointId);
        return true;
    }

    public async Task<DashboardStatsResponse> GetDashboardStatsAsync(
        CancellationToken cancellationToken
    )
    {
        if (
            memoryCache.TryGetValue<DashboardStatsResponse>(DashboardCacheKey, out var cached)
            && cached is not null
        )
        {
            return cached;
        }

        var devices = await dbContext.Devices.AsNoTracking().ToListAsync(cancellationToken);
        var dataPointCount = await dbContext
            .DataPoints.AsNoTracking()
            .CountAsync(cancellationToken);

        // 尝试获取 OPC Server 上报的实时状态
        memoryCache.TryGetValue<OpcServerStatusReport>(OpcStatusCacheKey, out var opcReport);

        var result = new DashboardStatsResponse(
            devices.Count,
            devices.Count(x => x.Type == DeviceType.Sensor),
            devices.Count(x => x.Type == DeviceType.Plc),
            dataPointCount,
            devices.Count(x => x.Status == DeviceStatus.Online),
            opcReport?.OnlineDevices ?? 0,
            opcReport?.FaultDevices ?? 0,
            opcReport?.TotalPoints ?? 0,
            opcReport?.IsEngineRunning ?? false
        );

        memoryCache.Set(DashboardCacheKey, result, CacheLifetime);
        return result;
    }

    private void InvalidateCache()
    {
        memoryCache.Remove(DevicesCacheKey);
        memoryCache.Remove(DashboardCacheKey);
    }

    private static DeviceDto ToDeviceDto(Device device)
    {
        return new DeviceDto(
            device.Id,
            device.Name,
            device.Type,
            device.ProtocolType,
            device.Status,
            device.ConnectionString,
            device.CreatedAtUtc,
            device.UpdatedAtUtc,
            device.DataPoints.OrderBy(x => x.Name).Select(ToDataPointDto).ToArray()
        );
    }

    private static DataPointDto ToDataPointDto(DataPoint point)
    {
        return new DataPointDto(
            point.Id,
            point.DeviceId,
            point.Address,
            point.Name,
            point.DataType,
            point.NodeId,
            point.NamespaceIndex,
            point.AlarmThreshold,
            point.UpdatedAtUtc
        );
    }
}
