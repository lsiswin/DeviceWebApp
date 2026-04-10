using DeviceApi.Application.Contracts;

namespace DeviceApi.Application.Interfaces;

public interface IDeviceService
{
    Task<IReadOnlyCollection<DeviceDto>> GetDevicesAsync(CancellationToken cancellationToken);

    Task<DeviceDto?> GetDeviceAsync(Guid id, CancellationToken cancellationToken);

    Task<DeviceDto> CreateDeviceAsync(CreateDeviceRequest request, CancellationToken cancellationToken);

    Task<DeviceDto?> UpdateDeviceAsync(Guid id, UpdateDeviceRequest request, CancellationToken cancellationToken);

    Task<bool> DeleteDeviceAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<DataPointDto>?> GetDataPointsAsync(Guid deviceId, CancellationToken cancellationToken);

    Task<DataPointDto?> GetDataPointAsync(Guid deviceId, Guid pointId, CancellationToken cancellationToken);

    Task<DataPointDto?> CreateDataPointAsync(Guid deviceId, CreateDataPointRequest request, CancellationToken cancellationToken);

    Task<DataPointDto?> UpdateDataPointAsync(Guid deviceId, Guid pointId, UpdateDataPointRequest request, CancellationToken cancellationToken);

    Task<bool> DeleteDataPointAsync(Guid deviceId, Guid pointId, CancellationToken cancellationToken);

    Task<DataPointDto?> UpdateDataPointValueAsync(Guid deviceId, Guid pointId, string value, CancellationToken cancellationToken);

    Task<DashboardStatsResponse> GetDashboardStatsAsync(CancellationToken cancellationToken);
}
