using DeviceApi.Application.Contracts;
using DeviceApi.Application.Interfaces;
using DeviceApi.Infrastructure.Identity;
using DeviceApi.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DeviceApi.Api.Controllers;

[ApiController]
[Route("api/admin/permissions")]
public class PermissionsController(
    UserManager<AppUser> userManager,
    IOperationLogService operationLogService) : ControllerBase
{
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMyPermissions(CancellationToken cancellationToken)
    {
        var userName = User.Identity?.Name ?? User.FindFirst("unique_name")?.Value ?? string.Empty;
        if (string.IsNullOrWhiteSpace(userName))
        {
            return Unauthorized();
        }

        var user = await userManager.FindByNameAsync(userName);
        if (user is null)
        {
            return NotFound();
        }

        var roles = await userManager.GetRolesAsync(user);
        var permissions = PermissionResolver.ResolvePermissions(roles);
        await operationLogService.WriteAsync("ReadPermissions", "User", user.Id, $"读取权限:{userName}", cancellationToken);

        return Ok(new PermissionSummaryResponse(userName, roles.ToArray(), permissions));
    }
}
