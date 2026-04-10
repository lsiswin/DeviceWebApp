using DeviceApi.Application.Contracts;
using DeviceApi.Application.Interfaces;
using DeviceApi.Domain.Entities;
using DeviceApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DeviceApi.Infrastructure.Services;

public class OperationLogService(
    AppDbContext dbContext,
    IHttpContextAccessor httpContextAccessor,
    ILogger<OperationLogService> logger) : IOperationLogService
{
    public async Task WriteAsync(string action, string resourceType, string resourceId, string detail, CancellationToken cancellationToken)
    {
        var userName = httpContextAccessor.HttpContext?.User?.Identity?.Name;
        if (string.IsNullOrWhiteSpace(userName))
        {
            userName = httpContextAccessor.HttpContext?.User?.FindFirst("unique_name")?.Value ?? "system";
        }

        var log = new OperationLog
        {
            Id = Guid.NewGuid(),
            UserName = userName,
            Action = action,
            ResourceType = resourceType,
            ResourceId = resourceId,
            Detail = detail,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        dbContext.OperationLogs.Add(log);
        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("操作日志记录: {Action} {ResourceType} {ResourceId}", action, resourceType, resourceId);
    }

    public async Task<IReadOnlyCollection<OperationLogDto>> GetLatestAsync(int take, CancellationToken cancellationToken)
    {
        var logs = await dbContext.OperationLogs
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(take)
            .ToListAsync(cancellationToken);

        return logs
            .Select(x => new OperationLogDto(
                x.Id,
                x.UserName,
                x.Action,
                x.ResourceType,
                x.ResourceId,
                x.Detail,
                x.CreatedAtUtc))
            .ToArray();
    }
}
