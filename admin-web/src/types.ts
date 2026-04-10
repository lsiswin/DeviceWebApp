export type DeviceType = 'Sensor' | 'Plc'

export interface DataPoint {
  id: string
  deviceId: string
  key: string
  name: string
  dataType: string
  value: string
  updatedAtUtc: string
}

export interface Device {
  id: string
  name: string
  type: DeviceType
  status: string
  createdAtUtc: string
  updatedAtUtc: string
  dataPoints: DataPoint[]
}

export interface DashboardStats {
  deviceCount: number
  sensorCount: number
  plcCount: number
  dataPointCount: number
  onlineDeviceCount: number
}

export interface AuthResponse {
  accessToken: string
  expiresAtUtc: string
  userName: string
  roles: string[]
}

export interface OperationLog {
  id: string
  userName: string
  action: string
  resourceType: string
  resourceId: string
  detail: string
  createdAtUtc: string
}

export interface PermissionSummary {
  userName: string
  roles: string[]
  permissions: string[]
}
