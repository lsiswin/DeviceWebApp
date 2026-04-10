# DeviceApi - API 文档（简体中文）

本文档面向需要调用或集成本服务的其他项目，覆盖本仓库中实现的所有 HTTP API：路由、方法、请求/响应格式、参数说明、权限要求与示例。

---

## 项目简介

DeviceApi 提供设备、数据点、操作日志、权限信息与认证相关的 HTTP API，常用于设备管理、仪表盘统计与 OPC 服务快照。服务以 REST 风格暴露接口，通过 JWT 等身份校验与基于策略的权限控制（Policy）限制访问。

---

## 全局约定
- 所有时间使用 ISO 8601（例如："2026-04-10T12:34:56Z"）或 .NET 的 DateTimeOffset 可序列化格式。
- 标识符：设备与数据点使用 GUID（示例：`"3fa85f64-5717-4562-b3fc-2c963f66afa6"`）。
- 错误与状态码：
  - 200 OK — 请求成功并返回数据。
  - 201 Created — 创建成功（资源位置在 Location header 或返回体）。
  - 204 No Content — 删除或无返回体的成功操作。
  - 400 Bad Request — 输入校验失败或语义错误。
  - 401 Unauthorized — 未认证。
  - 403 Forbidden — 已认证但没有权限。
  - 404 Not Found — 资源不存在。

---

## 认证与权限

- 登录后会返回 `AuthResponse`（见 DTO 部分），包含 `AccessToken`（通常为 JWT）和 `ExpiresAtUtc`。
- 控制器中使用的策略（Policy）与权限常量位于 `Application/Contracts/PermissionConstants.cs`：
  - AppPermissions.DeviceRead = "device:read"
  - AppPermissions.DeviceWrite = "device:write"
  - AppPermissions.DashboardRead = "dashboard:read"
  - AppPermissions.AuditRead = "audit:read"
  - AppPermissions.UserManage = "user:manage"

- 角色常量（AppRoles）：Admin、Operator、Viewer。权限到角色的解析通过项目内的 `PermissionResolver` 实现（调用方无需关心实现细节，只需关注返回的 permissions 列表）。

在下面每个接口描述中，会注明是否需要认证以及需要的策略。

---

## DTO（请求与响应数据结构）

下面列出项目中使用的主要契约（Contracts），类型与字段名如下：

- RegisterRequest
  - UserName: string
  - Password: string
  - Role: string

- LoginRequest
  - UserName: string
  - Password: string

- AuthResponse
  - AccessToken: string
  - ExpiresAtUtc: DateTimeOffset
  - UserName: string
  - Roles: IReadOnlyCollection<string>

- DeviceDto
  - Id: Guid
  - Name: string
  - Type: DeviceType (枚举，见项目 Domain.Enums)
  - Status: string
  - CreatedAtUtc: DateTimeOffset
  - UpdatedAtUtc: DateTimeOffset
  - DataPoints: IReadOnlyCollection<DataPointDto>

- DataPointDto
  - Id: Guid
  - DeviceId: Guid
  - Key: string
  - Name: string
  - DataType: string
  - Value: string
  - UpdatedAtUtc: DateTimeOffset

- CreateDeviceRequest
  - Name: string
  - Type: DeviceType
  - Status: string

- UpdateDeviceRequest
  - Name: string
  - Type: DeviceType
  - Status: string

- CreateDataPointRequest
  - Key: string
  - Name: string
  - DataType: string
  - Value: string

- UpdateDataPointRequest
  - Key: string
  - Name: string
  - DataType: string
  - Value: string

- UpdateDataPointValueRequest
  - Value: string

- DashboardStatsResponse
  - DeviceCount: int
  - SensorCount: int
  - PlcCount: int
  - DataPointCount: int
  - OnlineDeviceCount: int

- OperationLogDto
  - Id: Guid
  - UserName: string
  - Action: string
  - ResourceType: string
  - ResourceId: string
  - Detail: string
  - CreatedAtUtc: DateTimeOffset

- PermissionSummaryResponse
  - UserName: string
  - Roles: IReadOnlyCollection<string>
  - Permissions: IReadOnlyCollection<string>

---

## API 参考（按控制器）

注意：所有路由均以 `/api/...` 为前缀（见各控制器 Route 属性）。下面列出每个 endpoint 的路径、方法、参数、权限与示例。

### 1) 认证 - `AuthController`（路由: `/api/auth`）

- POST /api/auth/register
  - 权限：需要 `UserManage` 策略（AppPolicies.UserManage）
  - 请求体（JSON）: RegisterRequest
    {
      "userName": "alice",
      "password": "P@ssw0rd",
      "role": "Admin"
    }
  - 返回：
    - 200 OK -> { "message": "注册成功" }
    - 400 Bad Request -> { "errors": [...] }
  - 用途：管理员创建新用户并分配角色。

- POST /api/auth/login
  - 权限：AllowAnonymous（匿名可访问）
  - 请求体（JSON）: LoginRequest
    {
      "userName": "alice",
      "password": "P@ssw0rd"
    }
  - 返回：
    - 200 OK -> AuthResponse
      {
        "accessToken": "<jwt>",
        "expiresAtUtc": "2026-04-10T13:34:56Z",
        "userName": "alice",
        "roles": ["Admin"]
      }
    - 401 Unauthorized -> { "message": "用户名或密码错误" }
  - 用途：获取访问令牌以调用受保护的 API。


### 2) 管理设备 - `AdminDevicesController`（路由: `/api/admin/devices`）

- GET /api/admin/devices
  - 权限：Policy = DeviceRead
  - 参数：无
  - 返回：200 OK -> IReadOnlyCollection<DeviceDto>
  - 用途：获取所有设备及其数据点（完整 DTO）。

- GET /api/admin/devices/{id}
  - 权限：Policy = DeviceRead
  - 路径参数：id (GUID)
  - 返回：200 OK -> DeviceDto 或 404 Not Found
  - 用途：获取单个设备详情。

- POST /api/admin/devices
  - 权限：Policy = DeviceWrite
  - 请求体：CreateDeviceRequest
    {
      "name": "My Device",
      "type": 1, // DeviceType 枚举值
      "status": "Online"
    }
  - 返回：201 Created，Location 指向 GET 单个设备，响应体为创建后的 DeviceDto
  - 用途：创建新设备。

- PUT /api/admin/devices/{id}
  - 权限：Policy = DeviceWrite
  - 路径参数：id (GUID)
  - 请求体：UpdateDeviceRequest
    {
      "name": "Updated Name",
      "type": 1,
      "status": "Offline"
    }
  - 返回：200 OK -> 更新后的 DeviceDto，或 404 Not Found
  - 用途：更新设备属性。

- DELETE /api/admin/devices/{id}
  - 权限：Policy = DeviceWrite
  - 路径参数：id (GUID)
  - 返回：204 No Content（成功删除）或 404 Not Found
  - 用途：删除设备。

- GET /api/admin/devices/{deviceId}/datapoints
  - 权限：Policy = DeviceRead
  - 路径参数：deviceId (GUID)
  - 返回：200 OK -> IReadOnlyCollection<DataPointDto> 或 404 Not Found
  - 用途：获取某设备的全部数据点。

- POST /api/admin/devices/{deviceId}/datapoints
  - 权限：Policy = DeviceWrite
  - 路径参数：deviceId (GUID)
  - 请求体：CreateDataPointRequest
    {
      "key": "temp",
      "name": "Temperature",
      "dataType": "double",
      "value": "23.5"
    }
  - 返回：201 Created -> CreatedAtAction 返回创建的 DataPointDto，或 404 如果设备不存在
  - 用途：为设备添加数据点。

- GET /api/admin/devices/{deviceId}/datapoints/{pointId}
  - 权限：Policy = DeviceRead
  - 路径参数：deviceId, pointId (GUID)
  - 返回：200 OK -> DataPointDto 或 404
  - 用途：获取单个数据点详情。

- PUT /api/admin/devices/{deviceId}/datapoints/{pointId}
  - 权限：Policy = DeviceWrite
  - 路径参数：deviceId, pointId (GUID)
  - 请求体：UpdateDataPointRequest
    {
      "key": "temp",
      "name": "Temperature",
      "dataType": "double",
      "value": "23.5"
    }
  - 返回：200 OK -> 更新后的 DataPointDto 或 404
  - 用途：更新数据点元数据与当前值。

- DELETE /api/admin/devices/{deviceId}/datapoints/{pointId}
  - 权限：Policy = DeviceWrite
  - 路径参数：deviceId, pointId (GUID)
  - 返回：204 No Content 或 404
  - 用途：删除数据点。

- PUT /api/admin/devices/{deviceId}/datapoints/{pointId}/value
  - 权限：Policy = DeviceWrite
  - 路径参数：deviceId, pointId (GUID)
  - 请求体：UpdateDataPointValueRequest
    { "value": "42" }
  - 返回：200 OK -> 更新后的 DataPointDto 或 404
  - 用途：仅写入数据点的值（不修改 key/name/datatype）。


### 3) 仪表盘统计 - `DashboardController`（路由: `/api/admin/dashboard`）

- GET /api/admin/dashboard/stats
  - 权限：Policy = DashboardRead
  - 返回：200 OK -> DashboardStatsResponse
    {
      "deviceCount": 10,
      "sensorCount": 50,
      "plcCount": 2,
      "dataPointCount": 120,
      "onlineDeviceCount": 4
    }
  - 用途：前端仪表盘获取汇总统计数据。


### 4) OPC 快照 - `OpcController`（路由: `/api/opc`）
这些接口对匿名开放（AllowAnonymous），通常用于 OPC 网关或内网采集系统获取简化快照数据。

- GET /api/opc/devices
  - 权限：AllowAnonymous
  - 返回：200 OK -> 简化设备快照数组（包含 Id, Name, Type, Status）
  - 用途：提供给 OPC/采集服务快速获取设备列表。

- GET /api/opc/devices/{deviceId}/datapoints
  - 权限：AllowAnonymous
  - 路径参数：deviceId (GUID)
  - 返回：200 OK -> 简化数据点快照数组（Id, DeviceId, Key, Name, DataType, Value, UpdatedAtUtc）或 404
  - 用途：提供给 OPC/采集服务快速获取设备数据点当前值快照。


### 5) 权限摘要 - `PermissionsController`（路由: `/api/admin/permissions`）

- GET /api/admin/permissions/me
  - 权限：Authorize（任何已认证用户）
  - 返回：200 OK -> PermissionSummaryResponse
    {
      "userName": "alice",
      "roles": ["Admin"],
      "permissions": ["device:read","device:write", ...]
    }
  - 用途：客户端获取当前用户的角色与解析出的权限，用于界面展示与功能开关。

该请求还会写入一条操作日志（OperationLog）用于审计。


### 6) 操作日志 - `OperationLogsController`（路由: `/api/admin/operation-logs`）

- GET /api/admin/operation-logs?take={take}
  - 权限：Policy = AuditRead
  - 查询参数：take (int, 可选，默认 100, 范围 1-500)
  - 返回：200 OK -> IReadOnlyCollection<OperationLogDto>
  - 用途：获取最新的操作日志列表，用于审计与运维查看。


---

## 集成示例（调用流程）

1. 登录以获取访问令牌：POST /api/auth/login -> 保存 `accessToken`。
2. 在后续请求头中设置：
   Authorization: Bearer <accessToken>
3. 调用受保护接口（例如获取设备列表）并根据返回状态处理错误。

示例：获取设备列表（伪代码）
{
  method: "GET",
  url: "/api/admin/devices",
  headers: { "Authorization": "Bearer <token>" }
}

---

## 开发与运行（简要）

本仓库为 ASP.NET Core 应用，入口位于 `Program.cs`，配置文件为 `appsettings.json` 与 `appsettings.Development.json`。要在本地运行：
- 使用 .NET 10 SDK（从项目结构推断为 net10.0）
- dotnet run 或在 IDE（例如 Rider/Visual Studio）中启动
