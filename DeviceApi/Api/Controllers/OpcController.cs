using DeviceApi.Application.Contracts;
using DeviceApi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeviceApi.Api.Controllers;

[ApiController]
[Route("api/opc")]
public class OpcController(IDeviceService deviceService, IDataPointService dataPointService) : ControllerBase
{
    [HttpGet("devices")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDevices(CancellationToken cancellationToken)
    {
        var devices = await deviceService.GetDevicesAsync(cancellationToken);
        var snapshot = devices.Select(device => new
        {
            device.Id,
            device.Name,
            device.Type,
            device.ProtocolType,
            device.Status,
            device.ConnectionString,
        });
        return Ok(snapshot);
    }

    [HttpGet("devices/{deviceId:guid}/datapoints")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDataPoints(
        Guid deviceId,
        CancellationToken cancellationToken
    )
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
            point.Address,
            point.Name,
            point.DataType,
            point.NodeId,
            point.NamespaceIndex,
            point.UpdatedAtUtc,
        });
        return Ok(snapshot);
    }

    /// <summary>
    /// 获取单个数据点的NodeId
    /// </summary>
    [HttpGet("datapoints/{dataPointId:guid}/nodeid")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDataPointNodeId(
        Guid dataPointId,
        CancellationToken cancellationToken
    )
    {
        var dataPoint = await dataPointService.GetDataPointByIdAsync(dataPointId, cancellationToken);
        
        if (dataPoint == null)
        {
            return NotFound($"未找到数据点: {dataPointId}");
        }

        return Ok(new
        {
            dataPoint.Id,
            dataPoint.DeviceId,
            dataPoint.Name,
            dataPoint.NodeId,
            dataPoint.NamespaceIndex,
            dataPoint.UpdatedAtUtc
        });
    }

    /// <summary>
    /// 批量获取数据点的NodeId
    /// </summary>
    [HttpPost("datapoints/nodeid/batch-get")]
    [AllowAnonymous]
    public async Task<IActionResult> BatchGetDataPointNodeIds(
        [FromBody] BatchGetNodeIdRequest request,
        CancellationToken cancellationToken
    )
    {
        if (request == null || request.DataPointIds == null || request.DataPointIds.Count == 0)
        {
            return BadRequest("数据点ID列表不能为空");
        }

        var results = await dataPointService.BatchGetNodeIdsAsync(
            request.DataPointIds,
            cancellationToken
        );

        return Ok(results);
    }

    /// <summary>
    /// 更新数据点的NodeId (供OPC Server调用)
    /// </summary>
    [HttpPost("datapoints/{dataPointId:guid}/nodeid")]
    [Authorize] // 需要认证,确保只有授权的OPC Server可以调用
    public async Task<IActionResult> UpdateDataPointNodeId(
        Guid dataPointId,
        [FromBody] UpdateDataPointNodeIdRequest request,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(request.NodeId))
        {
            return BadRequest("NodeId不能为空");
        }

        var success = await dataPointService.UpdateNodeIdAsync(
            dataPointId,
            request.NodeId,
            request.NamespaceIndex,
            cancellationToken
        );

        if (!success)
        {
            return NotFound($"未找到数据点: {dataPointId}");
        }

        return Ok(new { message = "NodeId更新成功", dataPointId, request.NodeId });
    }

    /// <summary>
    /// 批量更新数据点的NodeId (供OPC Server批量注册节点后调用)
    /// </summary>
    [HttpPost("datapoints/nodeid/batch")]
    [Authorize]
    public async Task<IActionResult> BatchUpdateDataPointNodeIds(
        [FromBody] IReadOnlyCollection<UpdateNodeIdBatchRequest> requests,
        CancellationToken cancellationToken
    )
    {
        if (requests == null || requests.Count == 0)
        {
            return BadRequest("请求数据不能为空");
        }

        var result = await dataPointService.BatchUpdateNodeIdsAsync(
            requests,
            cancellationToken
        );

        return Ok(new
        {
            message = "批量更新完成",
            successCount = result.SuccessCount,
            failedCount = result.FailedCount,
            details = result.Details
        });
    }

    /// <summary>
    /// 上报OPC服务器整体状态
    /// </summary>
    [HttpPost("server/status")]
    [Authorize]
    public async Task<IActionResult> ReportServerStatus(
        [FromBody] OpcServerStatusReport report,
        CancellationToken cancellationToken
    )
    {
        await deviceService.ReportOpcStatusAsync(report, cancellationToken);
        return Ok(new { message = "状态上报成功" });
    }
}
