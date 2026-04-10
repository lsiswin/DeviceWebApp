using DeviceApi.Application.Contracts;
using DeviceApi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeviceApi.Api.Controllers;

[ApiController]
[Route("api/admin/dashboard")]
public class DashboardController(IDeviceService deviceService) : ControllerBase
{
    [HttpGet("stats")]
    [Authorize(Policy = AppPolicies.DashboardRead)]
    public async Task<IActionResult> GetStats(CancellationToken cancellationToken)
    {
        var stats = await deviceService.GetDashboardStatsAsync(cancellationToken);
        return Ok(stats);
    }
}
