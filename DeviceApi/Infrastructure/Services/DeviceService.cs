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
    ILogger<DeviceService> logger) : IDeviceService
{
    private const string DevicesCacheKey = "device:list";
    private const string DashboardCacheKey = "dashboard:stats";
    private static readonly TimeSpan CacheLifetime = TimeSpan.FromMinutes(5);

    public async Task<IReadOnlyCollection<DeviceDto>> GetDevicesAsync(CancellationToken cancellationToken)
    {
        if (memoryCache.TryGetValue<IReadOnlyCollection<DeviceDto>>(DevicesCacheKey, out var cached) && cached is not null)
        {
            return cached;
        }

        var devices = await dbContext.Devices
            .AsNoTracking()
            .Include(x => x.DataPoints)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        var result = devices.Select(ToDeviceDto).ToArray();
        memoryCache.Set(DevicesCacheKey, result, CacheLifetime);
        return result;
    }

    public async Task<DeviceDto?> GetDeviceAsync(Guid id, CancellationToken cancellationToken)
    {
        var device = await dbContext.Devices
            .AsNoTracking()
            .Include(x => x.DataPoints)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return device is null ? null : ToDeviceDto(device);
    }

    public async Task<DeviceDto> CreateDeviceAsync(CreateDeviceRequest request, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var entity = new Device
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Type = request.Type,
            Status = request.Status.Trim(),
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        dbContext.Devices.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        InvalidateCache();
        await operationLogService.WriteAsync("CreateDevice", "Device", entity.Id.ToString(), $"创建设备:{entity.Name}", cancellationToken);
        logger.LogInformation("设备已创建: {DeviceId} - {DeviceName}", entity.Id, entity.Name);

        return ToDeviceDto(entity);
    }

    public async Task<DeviceDto?> UpdateDeviceAsync(Guid id, UpdateDeviceRequest request, CancellationToken cancellationToken)
    {
        var device = await dbContext.Devices.Include(x => x.DataPoints).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (device is null)
        {
            return null;
        }

        device.Name = request.Name.Trim();
        device.Type = request.Type;
        device.Status = request.Status.Trim();
        device.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        InvalidateCache();
        await operationLogService.WriteAsync("UpdateDevice", "Device", id.ToString(), $"更新设备:{device.Name}", cancellationToken);
        logger.LogInformation("设备已更新: {DeviceId}", id);
        return ToDeviceDto(device);
    }

    public async Task<bool> DeleteDeviceAsync(Guid id, CancellationToken cancellationToken)
    {
        var device = await dbContext.Devices.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (device is null)
        {
            return false;
        }

        dbContext.Devices.Remove(device);
        await dbContext.SaveChangesAsync(cancellationToken);
        InvalidateCache();
        await operationLogService.WriteAsync("DeleteDevice", "Device", id.ToString(), $"删除设备:{device.Name}", cancellationToken);
        logger.LogInformation("设备已删除: {DeviceId}", id);
        return true;
    }

    public async Task<IReadOnlyCollection<DataPointDto>?> GetDataPointsAsync(Guid deviceId, CancellationToken cancellationToken)
    {
        var exists = await dbContext.Devices.AsNoTracking().AnyAsync(x => x.Id == deviceId, cancellationToken);
        if (!exists)
        {
            return null;
        }

        var points = await dbContext.DataPoints
            .AsNoTracking()
            .Where(x => x.DeviceId == deviceId)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return points.Select(ToDataPointDto).ToArray();
    }

    public async Task<DataPointDto?> GetDataPointAsync(Guid deviceId, Guid pointId, CancellationToken cancellationToken)
    {
        var point = await dbContext.DataPoints
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.DeviceId == deviceId && x.Id == pointId, cancellationToken);

        return point is null ? null : ToDataPointDto(point);
    }

    public async Task<DataPointDto?> CreateDataPointAsync(Guid deviceId, CreateDataPointRequest request, CancellationToken cancellationToken)
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
            Key = request.Key.Trim(),
            Name = request.Name.Trim(),
            DataType = request.DataType.Trim(),
            Value = request.Value.Trim(),
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };

        dbContext.DataPoints.Add(point);
        await dbContext.SaveChangesAsync(cancellationToken);
        InvalidateCache();
        await operationLogService.WriteAsync("CreateDataPoint", "DataPoint", point.Id.ToString(), $"创建数据点:{point.Key}", cancellationToken);
        logger.LogInformation("数据点已创建: {PointId} on device {DeviceId}", point.Id, deviceId);
        return ToDataPointDto(point);
    }

    public async Task<DataPointDto?> UpdateDataPointAsync(Guid deviceId, Guid pointId, UpdateDataPointRequest request, CancellationToken cancellationToken)
    {
        var point = await dbContext.DataPoints.FirstOrDefaultAsync(x => x.DeviceId == deviceId && x.Id == pointId, cancellationToken);
        if (point is null)
        {
            return null;
        }

        point.Key = request.Key.Trim();
        point.Name = request.Name.Trim();
        point.DataType = request.DataType.Trim();
        point.Value = request.Value.Trim();
        point.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        InvalidateCache();
        await operationLogService.WriteAsync("UpdateDataPoint", "DataPoint", pointId.ToString(), $"更新数据点:{point.Key}", cancellationToken);
        logger.LogInformation("数据点已更新: {PointId}", pointId);
        return ToDataPointDto(point);
    }

    public async Task<bool> DeleteDataPointAsync(Guid deviceId, Guid pointId, CancellationToken cancellationToken)
    {
        var point = await dbContext.DataPoints.FirstOrDefaultAsync(x => x.DeviceId == deviceId && x.Id == pointId, cancellationToken);
        if (point is null)
        {
            return false;
        }

        dbContext.DataPoints.Remove(point);
        await dbContext.SaveChangesAsync(cancellationToken);
        InvalidateCache();
        await operationLogService.WriteAsync("DeleteDataPoint", "DataPoint", pointId.ToString(), $"删除数据点:{point.Key}", cancellationToken);
        logger.LogInformation("数据点已删除: {PointId}", pointId);
        return true;
    }

    public async Task<DataPointDto?> UpdateDataPointValueAsync(Guid deviceId, Guid pointId, string value, CancellationToken cancellationToken)
    {
        var point = await dbContext.DataPoints.FirstOrDefaultAsync(x => x.DeviceId == deviceId && x.Id == pointId, cancellationToken);
        if (point is null)
        {
            return null;
        }

        point.Value = value.Trim();
        point.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        InvalidateCache();
        await operationLogService.WriteAsync("WriteDataPointValue", "DataPoint", pointId.ToString(), $"写入值:{value}", cancellationToken);
        logger.LogInformation("数据点值已写入: {PointId}", pointId);
        return ToDataPointDto(point);
    }

    public async Task<DashboardStatsResponse> GetDashboardStatsAsync(CancellationToken cancellationToken)
    {
        if (memoryCache.TryGetValue<DashboardStatsResponse>(DashboardCacheKey, out var cached) && cached is not null)
        {
            return cached;
        }

        var devices = await dbContext.Devices.AsNoTracking().ToListAsync(cancellationToken);
        var dataPointCount = await dbContext.DataPoints.AsNoTracking().CountAsync(cancellationToken);
        var result = new DashboardStatsResponse(
            devices.Count,
            devices.Count(x => x.Type == DeviceType.Sensor),
            devices.Count(x => x.Type == DeviceType.Plc),
            dataPointCount,
            devices.Count(x => x.Status == "在线")
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
            device.Status,
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
            point.Key,
            point.Name,
            point.DataType,
            point.Value,
            point.UpdatedAtUtc
        );
    }
}
