import type { AuthResponse, DashboardStats, DataPoint, Device, OperationLog, PermissionSummary } from '../types'

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5288'
const TOKEN_KEY = 'device_admin_token'
const USER_KEY = 'device_admin_user'
const ROLE_KEY = 'device_admin_roles'

export function getToken() {
  return localStorage.getItem(TOKEN_KEY) ?? ''
}

export function getCurrentUser() {
  return localStorage.getItem(USER_KEY) ?? ''
}

export function getCurrentRoles() {
  const rolesText = localStorage.getItem(ROLE_KEY)
  if (!rolesText) {
    return [] as string[]
  }

  try {
    return JSON.parse(rolesText) as string[]
  } catch {
    return []
  }
}

export function setAuth(auth: AuthResponse) {
  localStorage.setItem(TOKEN_KEY, auth.accessToken)
  localStorage.setItem(USER_KEY, auth.userName)
  localStorage.setItem(ROLE_KEY, JSON.stringify(auth.roles ?? []))
}

export function clearAuth() {
  localStorage.removeItem(TOKEN_KEY)
  localStorage.removeItem(USER_KEY)
  localStorage.removeItem(ROLE_KEY)
}

async function request<T>(path: string, init?: RequestInit) {
  const token = getToken()
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    ...(init?.headers as Record<string, string> | undefined)
  }

  if (token) {
    headers.Authorization = `Bearer ${token}`
  }

  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...init,
    headers
  })

  if (!response.ok) {
    const text = await response.text()
    throw new Error(text || `请求失败(${response.status})`)
  }

  if (response.status === 204) {
    return undefined as T
  }

  return (await response.json()) as T
}

export async function login(userName: string, password: string) {
  return request<AuthResponse>('/api/auth/login', {
    method: 'POST',
    body: JSON.stringify({ userName, password })
  })
}

export async function register(userName: string, password: string, role: string) {
  return request<{ message: string }>('/api/auth/register', {
    method: 'POST',
    body: JSON.stringify({ userName, password, role })
  })
}

export async function getDashboardStats() {
  return request<DashboardStats>('/api/admin/dashboard/stats')
}

export async function getDevices() {
  return request<Device[]>('/api/admin/devices')
}

export async function createDevice(payload: { name: string; type: string; status: string }) {
  return request<Device>('/api/admin/devices', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

export async function updateDevice(id: string, payload: { name: string; type: string; status: string }) {
  return request<Device>(`/api/admin/devices/${id}`, {
    method: 'PUT',
    body: JSON.stringify(payload)
  })
}

export async function deleteDevice(id: string) {
  return request<void>(`/api/admin/devices/${id}`, { method: 'DELETE' })
}

export async function getDataPoints(deviceId: string) {
  return request<DataPoint[]>(`/api/admin/devices/${deviceId}/datapoints`)
}

export async function createDataPoint(
  deviceId: string,
  payload: { key: string; name: string; dataType: string; value: string }
) {
  return request<DataPoint>(`/api/admin/devices/${deviceId}/datapoints`, {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

export async function updateDataPoint(
  deviceId: string,
  pointId: string,
  payload: { key: string; name: string; dataType: string; value: string }
) {
  return request<DataPoint>(`/api/admin/devices/${deviceId}/datapoints/${pointId}`, {
    method: 'PUT',
    body: JSON.stringify(payload)
  })
}

export async function deleteDataPoint(deviceId: string, pointId: string) {
  return request<void>(`/api/admin/devices/${deviceId}/datapoints/${pointId}`, {
    method: 'DELETE'
  })
}

export async function writeDataPointValue(deviceId: string, pointId: string, value: string) {
  return request<DataPoint>(`/api/admin/devices/${deviceId}/datapoints/${pointId}/value`, {
    method: 'PUT',
    body: JSON.stringify({ value })
  })
}

export async function getOperationLogs(take = 100) {
  return request<OperationLog[]>(`/api/admin/operation-logs?take=${take}`)
}

export async function getMyPermissions() {
  return request<PermissionSummary>('/api/admin/permissions/me')
}

export { API_BASE_URL }
