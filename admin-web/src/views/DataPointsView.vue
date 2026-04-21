<script setup lang="ts">
import { onMounted, reactive, ref, watch } from 'vue'
import {
  createDataPoint,
  deleteDataPoint,
  getDataPoints,
  getDevices,
  updateDataPoint
} from '../api/client'
import type { DataPoint, Device } from '../types'

const devices = ref<Device[]>([])
const selectedDeviceId = ref('')
const points = ref<DataPoint[]>([])
const loading = ref(false)
const message = ref('')

const form = reactive({
  id: '',
  address: '',
  name: '',
  dataType: 'double',
  alarmThreshold: null as number | null
})

function resetForm() {
  form.id = ''
  form.address = ''
  form.name = ''
  form.dataType = 'double'
  form.alarmThreshold = null
}

function editPoint(point: DataPoint) {
  form.id = point.id
  form.address = point.address
  form.name = point.name
  form.dataType = point.dataType
  form.alarmThreshold = point.alarmThreshold ?? null
}

async function loadDevices() {
  devices.value = await getDevices()
  if (!selectedDeviceId.value && devices.value.length > 0) {
    selectedDeviceId.value = devices.value[0].id
  }
}

async function loadDataPoints() {
  if (!selectedDeviceId.value) {
    points.value = []
    return
  }

  loading.value = true
  try {
    points.value = await getDataPoints(selectedDeviceId.value)
  } catch (error) {
    message.value = (error as Error).message
  } finally {
    loading.value = false
  }
}

async function submitForm() {
  if (!selectedDeviceId.value) {
    return
  }

  try {
    const payload = {
      address: form.address,
      name: form.name,
      dataType: form.dataType,
      alarmThreshold: form.alarmThreshold
    }
    if (form.id) {
      await updateDataPoint(selectedDeviceId.value, form.id, payload)
      message.value = '数据点更新成功'
    } else {
      await createDataPoint(selectedDeviceId.value, payload)
      message.value = '数据点创建成功'
    }
    resetForm()
    await loadDataPoints()
  } catch (error) {
    message.value = (error as Error).message
  }
}

async function onDelete(pointId: string) {
  if (!selectedDeviceId.value) {
    return
  }

  try {
    await deleteDataPoint(selectedDeviceId.value, pointId)
    message.value = '数据点删除成功'
    await loadDataPoints()
  } catch (error) {
    message.value = (error as Error).message
  }
}

watch(selectedDeviceId, () => {
  resetForm()
  loadDataPoints()
})

onMounted(async () => {
  try {
    await loadDevices()
    await loadDataPoints()
  } catch (error) {
    message.value = (error as Error).message
  }
})
</script>

<template>
  <div>
    <h1>数据点管理</h1>
    <p class="subtitle">仅维护点位定义，实时值由 OPC Server 采集</p>
    <p v-if="message" :class="['message', message.includes('成功') ? 'success' : 'error']">{{ message }}</p>
    <section class="panel">
      <label class="device-selector">
        选择设备
        <select v-model="selectedDeviceId">
          <option v-for="device in devices" :key="device.id" :value="device.id">
            {{ device.name }} ({{ device.type }} / {{ device.protocolType }})
          </option>
        </select>
      </label>
      <form class="form-grid" @submit.prevent="submitForm">
        <label>
          点位地址
          <input v-model="form.address" required />
        </label>
        <label>
          点位名称
          <input v-model="form.name" required />
        </label>
        <label>
          数据类型
          <input v-model="form.dataType" required />
        </label>
        <label>
          报警阈值
          <input v-model="form.alarmThreshold" type="number" step="any" placeholder="留空不监控限值" />
        </label>
        <div class="form-buttons">
          <button type="submit">{{ form.id ? '更新数据点' : '新增数据点' }}</button>
          <button type="button" class="secondary" @click="resetForm">清空</button>
        </div>
      </form>
    </section>

    <section class="panel">
      <p v-if="loading">加载中...</p>
      <table v-else>
        <thead>
          <tr>
            <th>名称</th>
            <th>地址</th>
            <th>类型</th>
            <th>超限阈值</th>
            <th>更新时间</th>
            <th>操作</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="point in points" :key="point.id">
            <td>{{ point.name }}</td>
            <td>{{ point.address }}</td>
            <td>{{ point.dataType }}</td>
            <td>{{ point.alarmThreshold !== null && point.alarmThreshold !== undefined ? point.alarmThreshold : '-' }}</td>
            <td>{{ new Date(point.updatedAtUtc).toLocaleString() }}</td>
            <td class="actions">
              <button class="secondary" @click="editPoint(point)">编辑</button>
              <button class="danger" @click="onDelete(point.id)">删除</button>
            </td>
          </tr>
        </tbody>
      </table>
    </section>
  </div>
</template>
