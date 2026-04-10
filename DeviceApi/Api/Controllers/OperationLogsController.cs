using DeviceApi.Application.Contracts;
using DeviceApi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeviceApi.Api.Controllers;

[ApiController]
[Route("api/admin/operation-logs")]
public class OperationLogsController(IOperationLogService operationLogService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = AppPolicies.AuditRead)]
    public async Task<IActionResult> GetLatest([FromQuery] int take = 100, CancellationToken cancellationToken = default)
    {
        var size = Math.Clamp(take, 1, 500);
        var logs = await operationLogService.GetLatestAsync(size, cancellationToken);
        return Ok(logs);
    }
}
