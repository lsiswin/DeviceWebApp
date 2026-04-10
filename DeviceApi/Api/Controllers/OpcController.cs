using DeviceApi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeviceApi.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/opc")]
public class OpcController(IDeviceService deviceService) : ControllerBase
{
    [HttpGet("devices")]
    public async Task<IActionResult> GetDevices(CancellationToken cancellationToken)
    {
        var devices = await deviceService.GetDevicesAsync(cancellationToken);
        var snapshot = devices.Select(device => new
        {
            device.Id,
            device.Name,
            device.Type,
            device.Status
        });
        return Ok(snapshot);
    }

    [HttpGet("devices/{deviceId:guid}/datapoints")]
    public async Task<IActionResult> GetDataPoints(Guid deviceId, CancellationToken cancellationToken)
    {
        var points = await deviceService.GetDataPointsAsync(deviceId, cancellationToken);
        if (points is null)
        {
            return NotFound();
        }

        var snapshot = points.Select(point => new
        {
            point.Id,
            point.DeviceId,
            point.Key,
            point.Name,
            point.DataType,
            point.Value,
            point.UpdatedAtUtc
        });
        return Ok(snapshot);
    }
}
