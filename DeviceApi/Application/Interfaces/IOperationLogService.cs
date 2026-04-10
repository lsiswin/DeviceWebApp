using DeviceApi.Application.Contracts;

namespace DeviceApi.Application.Interfaces;

public interface IOperationLogService
{
    Task WriteAsync(string action, string resourceType, string resourceId, string detail, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<OperationLogDto>> GetLatestAsync(int take, CancellationToken cancellationToken);
}
