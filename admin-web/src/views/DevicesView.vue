<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import { createDevice, deleteDevice, getDevices, updateDevice } from '../api/client'
import type { Device, DeviceType, ProtocolType } from '../types'

const devices = ref<Device[]>([])
const loading = ref(false)
const message = ref('')

const form = reactive({
  id: '',
  name: '',
  type: 'Sensor' as DeviceType,
  protocolType: 'ModbusTcp' as ProtocolType,
  status: 'Online',
  connectionString: ''
})

async function loadDevices() {
  loading.value = true
  message.value = ''
  try {
    devices.value = await getDevices()
  } catch (error) {
    message.value = (error as Error).message
  } finally {
    loading.value = false
  }
}

function resetForm() {
  form.id = ''
  form.name = ''
  form.type = 'Sensor'
  form.protocolType = 'ModbusTcp'
  form.status = 'Online'
  form.connectionString = ''
}

function editDevice(device: Device) {
  form.id = device.id
  form.name = device.name
  form.type = device.type
  form.protocolType = device.protocolType
  form.status = device.status
  form.connectionString = device.connectionString
}

async function submitForm() {
  try {
    const payload = {
      name: form.name,
      type: form.type,
      protocolType: form.protocolType,
      status: form.status,
      connectionString: form.connectionString
    }
    if (form.id) {
      await updateDevice(form.id, payload)
      message.value = '设备更新成功'
    } else {
      await createDevice(payload)
      message.value = '设备创建成功'
    }
    resetForm()
    await loadDevices()
  } catch (error) {
    message.value = (error as Error).message
  }
}

async function onDelete(id: string) {
  try {
    await deleteDevice(id)
    message.value = '设备删除成功'
    await loadDevices()
  } catch (error) {
    message.value = (error as Error).message
  }
}

onMounted(loadDevices)
</script>

<template>
  <div>
    <h1>设备管理</h1>
    <p class="subtitle">维护设备定义、协议类型和连接参数</p>
    <p v-if="message" :class="['message', message.includes('成功') ? 'success' : 'error']">{{ message }}</p>
    <section class="panel">
      <form class="form-grid" @submit.prevent="submitForm">
        <label>
          设备名称
          <input v-model="form.name" required />
        </label>
        <label>
          设备类型
          <select v-model="form.type">
            <option value="Sensor">Sensor</option>
            <option value="Plc">Plc</option>
          </select>
        </label>
        <label>
          协议类型
          <select v-model="form.protocolType">
            <option value="ModbusTcp">ModbusTcp</option>
            <option value="S7">S7</option>
          </select>
        </label>
        <label>
          设备状态
          <input v-model="form.status" required />
        </label>
        <label class="full-width">
          连接字符串
          <input v-model="form.connectionString" required />
        </label>
        <div class="form-buttons">
          <button type="submit">{{ form.id ? '更新设备' : '新增设备' }}</button>
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
            <th>类型</th>
            <th>协议</th>
            <th>状态</th>
            <th>数据点数量</th>
            <th>连接字符串</th>
            <th>更新时间</th>
            <th>操作</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="device in devices" :key="device.id">
            <td>{{ device.name }}</td>
            <td>{{ device.type }}</td>
            <td>{{ device.protocolType }}</td>
            <td>{{ device.status }}</td>
            <td>{{ device.dataPoints.length }}</td>
            <td>{{ device.connectionString }}</td>
            <td>{{ new Date(device.updatedAtUtc).toLocaleString() }}</td>
            <td class="actions">
              <button class="secondary" @click="editDevice(device)">编辑</button>
              <button class="danger" @click="onDelete(device.id)">删除</button>
            </td>
          </tr>
        </tbody>
      </table>
    </section>
  </div>
</template>
