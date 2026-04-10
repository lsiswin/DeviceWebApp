<script setup lang="ts">
import { onMounted, reactive, ref, watch } from 'vue'
import {
  createDataPoint,
  deleteDataPoint,
  getDataPoints,
  getDevices,
  updateDataPoint,
  writeDataPointValue
} from '../api/client'
import type { DataPoint, Device } from '../types'

const devices = ref<Device[]>([])
const selectedDeviceId = ref('')
const points = ref<DataPoint[]>([])
const loading = ref(false)
const message = ref('')

const form = reactive({
  id: '',
  key: '',
  name: '',
  dataType: 'double',
  value: ''
})

function resetForm() {
  form.id = ''
  form.key = ''
  form.name = ''
  form.dataType = 'double'
  form.value = ''
}

function editPoint(point: DataPoint) {
  form.id = point.id
  form.key = point.key
  form.name = point.name
  form.dataType = point.dataType
  form.value = point.value
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
    if (form.id) {
      await updateDataPoint(selectedDeviceId.value, form.id, {
        key: form.key,
        name: form.name,
        dataType: form.dataType,
        value: form.value
      })
      message.value = '数据点更新成功'
    } else {
      await createDataPoint(selectedDeviceId.value, {
        key: form.key,
        name: form.name,
        dataType: form.dataType,
        value: form.value
      })
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

async function onWrite(point: DataPoint) {
  if (!selectedDeviceId.value) {
    return
  }

  try {
    await writeDataPointValue(selectedDeviceId.value, point.id, point.value)
    message.value = '值写入成功'
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
    <p class="subtitle">维护设备数据点并通过后台写入值</p>
    <p v-if="message" :class="['message', message.includes('成功') ? 'success' : 'error']">{{ message }}</p>
    <section class="panel">
      <label class="device-selector">
        选择设备
        <select v-model="selectedDeviceId">
          <option v-for="device in devices" :key="device.id" :value="device.id">
            {{ device.name }} ({{ device.type }})
          </option>
        </select>
      </label>
      <form class="form-grid" @submit.prevent="submitForm">
        <label>
          点位Key
          <input v-model="form.key" required />
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
          当前值
          <input v-model="form.value" required />
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
            <th>Key</th>
            <th>类型</th>
            <th>值</th>
            <th>更新时间</th>
            <th>操作</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="point in points" :key="point.id">
            <td>{{ point.name }}</td>
            <td>{{ point.key }}</td>
            <td>{{ point.dataType }}</td>
            <td>
              <input v-model="point.value" />
            </td>
            <td>{{ new Date(point.updatedAtUtc).toLocaleString() }}</td>
            <td class="actions">
              <button class="secondary" @click="editPoint(point)">编辑</button>
              <button @click="onWrite(point)">写入</button>
              <button class="danger" @click="onDelete(point.id)">删除</button>
            </td>
          </tr>
        </tbody>
      </table>
    </section>
  </div>
</template>
