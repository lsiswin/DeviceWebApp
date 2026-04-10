# DeviceWebApp 运行说明

## 1. 项目结构

- `DeviceApi`：.NET 10 后端（分层架构，Identity + EF Core + SQL Server + Serilog + MemoryCache）
- `admin-web`：Vue3 管理后台（多页面，权限中心，操作日志页面）
- `docker-compose.yml`：一键启动 SQL Server + 后端 + 前端

## 2. 本地运行

### 2.1 启动后端

```powershell
cd DeviceApi
dotnet restore
dotnet run
```

后端默认地址：

- `http://localhost:5288`

### 2.2 启动前端

```powershell
cd admin-web
npm install
npm run dev
```

前端默认地址：

- `http://localhost:5173`

## 3. Docker 运行

在项目根目录执行：

```powershell
docker compose up --build
```

启动后地址：

- 前端：`http://localhost:5173`
- 后端：`http://localhost:5288`
- SQL Server：`localhost,1433`

## 4. 默认账号

- `admin / Admin@123456`（管理员，拥有全部权限）
- `operator / Operator@123456`（操作员，可读写设备与数据点，可查看操作日志）
- `viewer / Viewer@123456`（只读用户，可查看仪表盘和设备数据）

## 5. 权限与日志

- 权限策略：
  - `DeviceRead`：Admin / Operator / Viewer
  - `DeviceWrite`：Admin / Operator
  - `DashboardRead`：Admin / Operator / Viewer
  - `AuditRead`：Admin / Operator
  - `UserManage`：Admin
- 操作日志写入场景：
  - 登录、注册用户
  - 设备新增/修改/删除
  - 数据点新增/修改/删除
  - 数据点写值
  - 权限查询
- 日志查询接口：
  - `GET /api/admin/operation-logs?take=100`

## 6. 常用接口

- 登录：`POST /api/auth/login`
- 管理端设备：`/api/admin/devices`
- 管理端数据点：`/api/admin/devices/{deviceId}/datapoints`
- 仪表盘：`GET /api/admin/dashboard/stats`
- 权限信息：`GET /api/admin/permissions/me`
- OPC 只读：`/api/opc/devices`、`/api/opc/devices/{deviceId}/datapoints`

## 7. 构建验证命令

```powershell
cd DeviceApi
dotnet build

cd ../admin-web
npm run build
```
