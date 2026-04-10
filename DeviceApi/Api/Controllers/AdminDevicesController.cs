using DeviceApi.Application.Contracts;
using DeviceApi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeviceApi.Api.Controllers;

[ApiController]
[Route("api/admin/devices")]
public class AdminDevicesController(IDeviceService deviceService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = AppPolicies.DeviceRead)]
    public async Task<IActionResult> GetDevices(CancellationToken cancellationToken)
    {
        var result = await deviceService.GetDevicesAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = AppPolicies.DeviceRead)]
    public async Task<IActionResult> GetDevice(Guid id, CancellationToken cancellationToken)
    {
        var result = await deviceService.GetDeviceAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = AppPolicies.DeviceWrite)]
    public async Task<IActionResult> CreateDevice([FromBody] CreateDeviceRequest request, CancellationToken cancellationToken)
    {
        var created = await deviceService.CreateDeviceAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetDevice), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = AppPolicies.DeviceWrite)]
    public async Task<IActionResult> UpdateDevice(Guid id, [FromBody] UpdateDeviceRequest request, CancellationToken cancellationToken)
    {
        var updated = await deviceService.UpdateDeviceAsync(id, request, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = AppPolicies.DeviceWrite)]
    public async Task<IActionResult> DeleteDevice(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await deviceService.DeleteDeviceAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    [HttpGet("{deviceId:guid}/datapoints")]
    [Authorize(Policy = AppPolicies.DeviceRead)]
    public async Task<IActionResult> GetDataPoints(Guid deviceId, CancellationToken cancellationToken)
    {
        var points = await deviceService.GetDataPointsAsync(deviceId, cancellationToken);
        return points is null ? NotFound() : Ok(points);
    }

    [HttpPost("{deviceId:guid}/datapoints")]
    [Authorize(Policy = AppPolicies.DeviceWrite)]
    public async Task<IActionResult> CreateDataPoint(Guid deviceId, [FromBody] CreateDataPointRequest request, CancellationToken cancellationToken)
    {
        var point = await deviceService.CreateDataPointAsync(deviceId, request, cancellationToken);
        return point is null
            ? NotFound()
            : CreatedAtAction(nameof(GetDataPoint), new { deviceId, pointId = point.Id }, point);
    }

    [HttpGet("{deviceId:guid}/datapoints/{pointId:guid}")]
    [Authorize(Policy = AppPolicies.DeviceRead)]
    public async Task<IActionResult> GetDataPoint(Guid deviceId, Guid pointId, CancellationToken cancellationToken)
    {
        var point = await deviceService.GetDataPointAsync(deviceId, pointId, cancellationToken);
        return point is null ? NotFound() : Ok(point);
    }

    [HttpPut("{deviceId:guid}/datapoints/{pointId:guid}")]
    [Authorize(Policy = AppPolicies.DeviceWrite)]
    public async Task<IActionResult> UpdateDataPoint(Guid deviceId, Guid pointId, [FromBody] UpdateDataPointRequest request, CancellationToken cancellationToken)
    {
        var point = await deviceService.UpdateDataPointAsync(deviceId, pointId, request, cancellationToken);
        return point is null ? NotFound() : Ok(point);
    }

    [HttpDelete("{deviceId:guid}/datapoints/{pointId:guid}")]
    [Authorize(Policy = AppPolicies.DeviceWrite)]
    public async Task<IActionResult> DeleteDataPoint(Guid deviceId, Guid pointId, CancellationToken cancellationToken)
    {
        var deleted = await deviceService.DeleteDataPointAsync(deviceId, pointId, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPut("{deviceId:guid}/datapoints/{pointId:guid}/value")]
    [Authorize(Policy = AppPolicies.DeviceWrite)]
    public async Task<IActionResult> WriteValue(Guid deviceId, Guid pointId, [FromBody] UpdateDataPointValueRequest request, CancellationToken cancellationToken)
    {
        var point = await deviceService.UpdateDataPointValueAsync(deviceId, pointId, request.Value, cancellationToken);
        return point is null ? NotFound() : Ok(point);
    }
}
